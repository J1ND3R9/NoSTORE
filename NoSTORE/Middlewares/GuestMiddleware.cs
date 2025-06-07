using NoSTORE.Services;
using Org.BouncyCastle.Asn1.Ocsp;

namespace NoSTORE.Middlewares
{
    public class GuestMiddleware : IMiddleware
    {
        private readonly UserService _userService;
        
        public GuestMiddleware(UserService userService)
        {
            _userService = userService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            var isMobile = remoteIp == "10.0.2.2" || userAgent.Contains("okhttp", StringComparison.OrdinalIgnoreCase);

            if (isMobile)
            {
                await next(context);
                return;
            }

            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            var hasGuestCookie = context.Request.Cookies.ContainsKey("GuestId");

            if (!isAuthenticated && !hasGuestCookie)
            {
                var guest = await _userService.CreateGuest();

                context.Response.Cookies.Append("GuestId", guest.Id.ToString(), new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    IsEssential = true,
                    HttpOnly = true
                });

                context.Response.Headers.Append("X-Guest-Id", guest.Id.ToString());
            }

            await next(context);
        }
    }
}
