using Microsoft.AspNetCore.SignalR;

namespace NoSTORE.Hubs
{
    public class UserHub : Hub
    {
        public async Task NotifyCartChanged(string userId)
        {
            await Clients.User(userId).SendAsync("CartChanged");
        }

        public async Task NotifyFavoriteChanged(string userId)
        {
            await Clients.User(userId).SendAsync("FavoriteChanged");
        }
    }
}
