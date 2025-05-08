using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NoSTORE.Models;
using NoSTORE.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace NoSTORE.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Route("~/profile/{section?}")]
        public async Task<IActionResult> Index(string section = "settings")
        {
            ViewBag.Section = section;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);

            var model = new UserProfileSafe(
                user.Id,
                user.Nickname,
                user.Avatar,
                user.Email,
                user.Phone,
                user.RoleId,
                user.Favorites,
                user.Basket,
                user.PCs,
                user.Orders,
                user.Reviews
            );

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView(GetPartialViewName(section), model);
            }

            return View(model);
        }

        private string GetPartialViewName(string section)
        {
            return section switch
            {
                "basket" => "Partials/_Basket",
                "favorites" => "Partials/_Favorites",
                "pcs" => "Partials/_Pcs",
                "orders" => "Partials/_Orders",
                _ => "Partials/_Settings"
            };
        }

        private string FirstLetterUpper(string word) =>
            char.ToUpper(word[0]) + word.Substring(1);
    }
}
