using NoSTORE.Models;

namespace NoSTORE.Models.DTO
{
    public record UserDto(
        string Id,
        string Nickname,
        string Avatar,
        string Email,
        string? Phone,
        string RoleId,
        List<string> Favorites,
        List<User.BasketItem> Basket,
        List<string> PCs,
        List<string> Orders,
        List<string> Reviews
        );
}
