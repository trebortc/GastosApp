using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GastosApp.Api.Extensions;

/// <summary>
/// Extensiones de ClaimsPrincipal para extraer datos del usuario autenticado
/// a partir del JWT. El UserId siempre viene del token, nunca del body.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token sin claim de usuario.");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Claim de usuario invalido.");
    }

    public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }
}
