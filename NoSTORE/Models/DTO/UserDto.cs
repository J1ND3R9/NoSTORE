using NoSTORE.Models;

namespace NoSTORE.Models.DTO
{
    public record UserDto(
        string Id,
        string Nickname,
        string? AvatarExt,
        string Email,
        string? Phone,
        string RoleId,
        List<string> Favorites,
        List<User.BasketItem> Basket,
        List<string> PCs,
        List<Order> Orders,
        List<Review> Reviews,
        DateTime RegistrationDate
        );
}
