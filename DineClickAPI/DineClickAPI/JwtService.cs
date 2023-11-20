using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DineClickAPI;

public class JwtService
{
    private readonly string? _audience;
    private readonly string? _issuer;
    private readonly SymmetricSecurityKey? _authSigningKey;

    public JwtService(IConfiguration configuration)
    {
        _audience = configuration["Jwt:ValidAudience"];
        _issuer = configuration["Jwt:ValidIssuer"];
        _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
    }

    public string CreateAccessToken(string userId, string username, string role)
    {
        var authClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.Sub, userId),
            new (ClaimTypes.Name, username),
            new (ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken
        (
            audience: _audience,
            issuer: _issuer,
            expires: DateTime.UtcNow.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken(string userId)
    {
        var authClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (JwtRegisteredClaimNames.Sub, userId)
        };
        var token = new JwtSecurityToken
        (
            audience: _audience,
            issuer: _issuer,
            expires: DateTime.UtcNow.AddHours(24),
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims)
    {
        claims = null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = _audience,
                ValidIssuer = _issuer,
                IssuerSigningKey = _authSigningKey,
                ValidateLifetime = true
            };
            claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
