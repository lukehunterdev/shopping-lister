using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.SettingsModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LhDev.ShoppingLister.Services;

public interface IJwtService
{
    string GenerateToken(User user, DateTime? expires = null);
}

public class JwtService(JwtSettings jwtSettings) : IJwtService
{
    public string GenerateToken(User user, DateTime? expires = null)
    {
        // Use values from Jwt settings section for duration until expiration and key.
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            Expires = expires ?? DateTime.UtcNow.AddSeconds(jwtSettings.Duration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Claims = new Dictionary<string, object>
            {
                { "user", JsonSerializer.Serialize(user) },
            },
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}