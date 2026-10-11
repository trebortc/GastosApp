using GastosApp.Application.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GastosApp.Infrastructure.Authentication;

/// <summary>
/// Implementacion del servicio JWT.
/// La clave secreta se lee de la variable de entorno GASTOSAPP_JWT_KEY
/// o de Jwt:Key en la configuracion (User Secrets en desarrollo).
/// Nunca la pongas en appsettings.json con un valor real.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public int AccessTokenLifetimeMinutes { get; }
    public int RefreshTokenLifetimeDays { get; }

    public JwtTokenService(IConfiguration configuration)
    {
        var key = Environment.GetEnvironmentVariable("GASTOSAPP_JWT_KEY")
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No se encontro la clave JWT. Define GASTOSAPP_JWT_KEY o Jwt:Key en User Secrets.");

        if (key.Length < 32)
            throw new InvalidOperationException(
                "La clave JWT debe tener al menos 32 caracteres.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _issuer = configuration["Jwt:Issuer"] ?? "GastosApp";
        _audience = configuration["Jwt:Audience"] ?? "GastosApp.Mobile";
        AccessTokenLifetimeMinutes = int.TryParse(configuration["Jwt:AccessTokenLifetimeMinutes"], out var atl) ? atl : 60;
        RefreshTokenLifetimeDays = int.TryParse(configuration["Jwt:RefreshTokenLifetimeDays"], out var rtl) ? rtl : 30;
    }

    public string GenerateAccessToken(Guid userId, string email, string displayName, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public Guid? GetUserIdFromExpiredToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = false // Permite tokens expirados para refresh
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(accessToken, tokenValidationParameters, out _);

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }
}
