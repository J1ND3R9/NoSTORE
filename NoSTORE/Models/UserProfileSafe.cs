namespace NoSTORE.Models
{
    public record UserProfileSafe(
        string Id,
        string Nickname,
        string Avatar,
        string Email,
        string? Phone,
        string RoleId,
        List<User.FavoriteItem> Favorites,
        List<User.BasketItem> Basket,
        List<string> PCs,
        List<string> Orders,
        List<string> Reviews
        );
}
