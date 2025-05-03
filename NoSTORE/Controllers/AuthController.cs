using BCrypt.Net;
using DnsClient;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MongoDB.Driver;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Net.Mail;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService,
            JwtService jwtService,
            EmailService emailService,
            VerificationService verificationService,
            RoleService roleService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _emailService = emailService;
            _verificationService = verificationService;
            _roleService = roleService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model, CancellationToken ct)
        {
            if (await CheckLogin(model))
                return BadRequest("Неверная почта или пароль");
            if (!await _verificationService.IsValidCodeAsync(model.Email, model.Code))
                return Unauthorized("Неверный код");
            var user = await _userService.GetUserByEmailAsync(model.Email);
            var jwt = _jwtService.GenerateToken(user);
            return Ok(jwt);
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
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password),
                Nickname = model.Nickname
            };
            await _userService.InsertUser(user);
            var jwt = _jwtService.GenerateToken(user);
            return Ok(jwt);
        }

        [HttpPost("login/send-code")]
        public async Task<IActionResult> LoginSend([FromBody] Login model, CancellationToken ct)
        {
            if (await CheckLogin(model))
                return Unauthorized("Неверная почта или пароль");
            if (!await _verificationService.SendVerificationCodeAsync(model.Email, ct))
                return Unauthorized("Ошибка на стороне сервера, обратитесь к администратору");
            return Ok();
        }

        [HttpPost("register/send-code")]
        public async Task<IActionResult> RegisterSend([FromBody] Register model, CancellationToken ct)
        {
            if (await RegisterEmailIsExist(model))
                return Unauthorized("Пользователь уже существует");
            if (!await _verificationService.SendVerificationCodeAsync(model.Email, ct))
                return Unauthorized("Ошибка на стороне сервера, обратитесь к администратору");
            return Ok();
        }

        private async Task<bool> CheckLogin(Login model)
        {
            var user = await _userService.GetUserByEmailAsync(model.Email);
            return user == null || !BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.PasswordHash);
        }

        private async Task<bool> RegisterEmailIsExist(Register model) => await _userService.UserExistsByEmail(model.Email);
    }
}
