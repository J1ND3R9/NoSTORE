using BCrypt.Net;
using DnsClient;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSTORE.Models;
using NoSTORE.Services;
using NoSTORE.Settings;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly VerificationService _verificationService;
        private readonly RoleService _roleService;
        private readonly AuthSettings _authSettings;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(UserService userService,
            JwtService jwtService,
            EmailService emailService,
            VerificationService verificationService,
            RoleService roleService,
            ILogger<AuthController> logger,
            IWebHostEnvironment env)
        {
            _userService = userService;
            _jwtService = jwtService;
            _emailService = emailService;
            _verificationService = verificationService;
            _roleService = roleService;
            _logger = logger;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model, CancellationToken ct)
        {
            if (await CheckLogin(model))
                return BadRequest(new { error = "Неверная почта или пароль" });
            var userAgent = Request.Headers["User-Agent"].ToString();
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var isMobile = remoteIp == "10.0.2.2" || userAgent.Contains("okhttp", StringComparison.OrdinalIgnoreCase);

            if (!isMobile)
            {
                if (!await _verificationService.IsValidCodeAsync(model.Email, model.Code))
                    return BadRequest(new { error = "Неверный код" });
            }
            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (Request.Cookies.TryGetValue("GuestId", out var guestId))
            {
                await _userService.DeleteUserAsync(guestId);
                Response.Cookies.Delete("GuestId");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (isMobile)
            {
                var jwtService = HttpContext.RequestServices.GetRequiredService<JwtService>();
                var token = jwtService.GenerateToken(claims);

                return Ok(new
                {
                    token,
                    userId = user.Id
                });
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
                );

            return Ok();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(model.Nickname))
                return BadRequest("Не ввели никнейм");
            if (await RegisterEmailIsExist(model))
                return Unauthorized("Пользователь уже существует");
            if (!await _verificationService.IsValidCodeAsync(model.Email, model.Code))
                return Unauthorized("Неверный код");
            var user = new User()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password),
                Nickname = model.Nickname,
                RegistrationDate = DateTime.UtcNow
            };

            if (Request.Cookies.TryGetValue("GuestId", out var guestId))
            {
                var guest = await _userService.GetUserById(guestId);

                user.PCs = guest.PCs;
                user.Compares = guest.Compares;
                user.Orders = guest.Orders;
                user.Favorites = guest.Favorites;
                user.Basket = guest.Basket;

                await _userService.DeleteUserAsync(guestId);
                Response.Cookies.Delete("GuestId");
            }

            await _userService.InsertUser(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var userAgent = Request.Headers["User-Agent"].ToString();
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var isMobile = remoteIp == "10.0.2.2" || userAgent.Contains("okhttp", StringComparison.OrdinalIgnoreCase);

            if (isMobile)
            {
                var jwtService = HttpContext.RequestServices.GetRequiredService<JwtService>();
                var token = jwtService.GenerateToken(claims);

                return Ok(new
                {
                    token,
                    userId = user.Id
                });
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
                );
            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var guest = await _userService.CreateGuest();

            HttpContext.Response.Cookies.Append("GuestId", guest.Id.ToString(), new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7),
                IsEssential = true,
                HttpOnly = true
            });

            return Ok();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (!User.Identity.IsAuthenticated)
                return Ok(new
                {
                    isAuthenticated = false
                });
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nickname = await _userService.GetNickname(userId);
            var avatar = await _userService.GetAvatarExtension(userId);
            var role = await _userService.GetRole(userId);

            return Ok(new
            {
                isAuthenticated = true,
                userId,
                avatar,
                nickname,
                role
            });
        }

        [HttpPost("login/try-send-code")]
        public async Task<IActionResult> LoginSend([FromBody] Login model, CancellationToken ct)
        {
            if (await CheckLogin(model))
                return BadRequest(new { error = "Неверная почта или пароль" });
            if (!await _verificationService.SendVerificationCodeAsync(model.Email, ct))
                return Unauthorized("Ошибка на стороне сервера, обратитесь к администратору");
            return Ok();
        }

        [HttpPost("register/try-send-code")]
        public async Task<IActionResult> RegisterSend([FromBody] Register model, CancellationToken ct)
        {
            if (await RegisterEmailIsExist(model))
                return BadRequest(new { error = "Пользователь уже существует" });
            if (!await _verificationService.SendVerificationCodeAsync(model.Email, ct))
                return BadRequest("Ошибка на стороне сервера, обратитесь к администратору");
            return Ok();
        }

        [Authorize]
        [HttpPost("change/send-code")]
        public async Task<IActionResult> SendCode(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);
            if (user == null)
                return BadRequest("Пользователь не найден");
            if (!await _verificationService.SendVerificationCodeAsync(user.Email, ct))
                return BadRequest("Ошибка на стороне сервера, обратитесь к администратору");
            return Ok();
        }

        [Authorize]
        [HttpPost("change/email_exist")]
        public async Task<IActionResult> EmailIsExist([FromBody] ChangeDto dto)
        {
            var exist = await _userService.UserExistsByEmail(dto.Email);
            if (exist)
                return BadRequest("Пользователь с таким Email уже существует");
            return Ok();
        }

        [Authorize]
        [HttpPost("change/email")]
        public async Task<IActionResult> EmailChange([FromBody] ChangeDto dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);
            if (user == null)
                return BadRequest("Пользователь не найден");
            if (!await _verificationService.IsValidCodeAsync(user.Email, dto.Code ?? "-"))
                return BadRequest("Неверный код");
            if (dto.Email == null)
                return BadRequest("Проблемы с почтой");
            await _userService.ChangeEmail(userId, dto.Email);
            return Ok();
        }

        [Authorize]
        [HttpPost("change/nickname")]
        public async Task<IActionResult> NicknameChange([FromBody] ChangeDto dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);
            if (user == null)
                return BadRequest("Пользователь не найден");
            if (dto.Nickname == null)
                return BadRequest("Проблемы с никнеймом");
            await _userService.ChangeNickname(userId, dto.Nickname);
            return Ok();
        }

        [Authorize]
        [HttpPost("change/password")]
        public async Task<IActionResult> PasswordChange([FromBody] ChangeDto dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);
            if (user == null)
                return BadRequest("Пользователь не найден");
            if (!await _verificationService.IsValidCodeAsync(user.Email, dto.Code ?? "-"))
                return BadRequest("Неверный код");
            var passwordhash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.NewPassword);
            await _userService.ChangePassword(userId, passwordhash);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [Authorize]
        [HttpPost("change/check-password")]
        public async Task<IActionResult> PasswordCheck([FromBody] ChangeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);
            if (user == null)
                return BadRequest("Пользовател не найден");

            if (!BCrypt.Net.BCrypt.EnhancedVerify(dto.OldPassword, user.PasswordHash))
                return BadRequest("Неверный пароль");

            if (dto.OldPassword == dto.NewPassword)
                return BadRequest("Пароли совпадают");

            if (!IsValidPassword(dto.NewPassword))
                return BadRequest("Новый пароль не проходит валидацию.");

            return Ok();
        }

        [Authorize]
        [HttpPost("change/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest("Файл не выбран");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".png", ".jpeg", "webp", ".jfif" }.Contains(ext))
                return BadRequest("Недопустимый формат изображения");

            var uploadFolder = Path.Combine(_env.WebRootPath, "photos", "avatars");
            var files = Directory.GetFiles(uploadFolder, $"{userId}.*");
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось удалить файл {file}: {ex.Message}");
                }
            }

            var filename = $"{userId}{ext}";
            var path = Path.Combine(uploadFolder, filename);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            await _userService.ChangeAvatar(userId, ext);

            return Ok();
        }

        private async Task<bool> CheckLogin(Login model)
        {
            var user = await _userService.GetUserByEmailAsync(model.Email);
            return user == null || !BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.PasswordHash);
        }

        private async Task<bool> RegisterEmailIsExist(Register model) => await _userService.UserExistsByEmail(model.Email);

        bool IsValidPassword(string password)
        {
            var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).{6,}$");
            return regex.IsMatch(password);
        }
    }
    public class ChangeDto
    {
        public string? Email { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? Nickname { get; set; }
        public string? Code { get; set; }
    }
}
