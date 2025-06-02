using NoSTORE.Services;

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
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated && !context.Request.Cookies.ContainsKey("GuestId"))
            {
                var guest = await _userService.CreateGuest();

                context.Response.Cookies.Append("GuestId", guest.Id.ToString(), new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    IsEssential = true,
                    HttpOnly = true
                });
            }

            await next(context);
        }
    }
}
