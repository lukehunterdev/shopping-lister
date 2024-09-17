using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.SettingsModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LhDev.ShoppingLister.Middleware;

public class JwtMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        // Auth header should look something like this "Bearer asdfbjnkasdffyugjasekugfgdkjlnhgfdinb"
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader[7..];
            ValidateAndAttachUserToContext(context, token, "bearer");
            
            await next(context);

            return;
        }

        // Check for cookie
        var cookieHeader = context.Request.Cookies[Program.CookieSession];
        if (!string.IsNullOrEmpty(cookieHeader)) ValidateAndAttachUserToContext(context, cookieHeader, "cookie");

        await next(context);
    }


    private void ValidateAndAttachUserToContext(HttpContext context, string token, string source)
    {
        var jwtSettings = Program.JwtSettings;

        try
        {
            context.Items["JwtSource"] = source;
            // Validate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Key);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                // By default tokens expire 5 mins after expiry datetime, but change this to 0.
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            // Get user from claims.
            var jwtToken = (JwtSecurityToken)validatedToken;
            var user = JsonSerializer.Deserialize<User>(jwtToken.Claims.First(x => x.Type == "user").Value);

            // Set the user object to this context
            context.Items["User"] = user;
        }
        catch (SecurityTokenExpiredException)
        {
            context.Items["FailReason"] = "JWT has expired.";
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            context.Items["FailReason"] = "Provided JWT has invalid signature.";
        }
        catch
        {
            // Validation failed for whatever other reason, so swallow exception.
            context.Items["FailReason"] = "Error authorising JWT.";
        }
    }
}