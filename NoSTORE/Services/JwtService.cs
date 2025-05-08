using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoSTORE.Models;
using NoSTORE.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NoSTORE.Services
{
    public class JwtService(IOptions<AuthSettings> options)
    {
        DateTime expires = DateTime.UtcNow.Add(options.Value.Expires);

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nickname),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };
            var jwtToken = new JwtSecurityToken(
                issuer: options.Value.Issuer,
                audience: options.Value.Audience,
                expires: expires,
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            options.Value.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
