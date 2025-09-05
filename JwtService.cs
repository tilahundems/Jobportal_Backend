namespace JobPortalAPI;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public interface IJwtService
{
    string GenerateToken(IEnumerable<Claim> claims);
    ClaimsPrincipal ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }
   

private string SanitizeClaimValue(string value)
{
    if (string.IsNullOrEmpty(value)) return value;
    
    // Remove any characters that could break JWT format
    return value.Replace("\n", "")
               .Replace("\r", "")
               .Replace("\t", "")
               .Replace(" ", ""); // Remove spaces too for safety
}


    public string GenerateToken(IEnumerable<Claim> claims)
    {

// SANITIZE all claim values first
    var sanitizedClaims = claims.Select(c => new Claim(
        c.Type, 
        SanitizeClaimValue(c.Value) // Remove any problematic characters
    ));
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _config.GetValue<int>("Jwt:ExpiryInMinutes", 60)
            ),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var principal = tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _config["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        return principal;
    }
}