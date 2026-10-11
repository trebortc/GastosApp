namespace GastosApp.Application.Authentication;

/// <summary>
/// Contrato para generacion y validacion de tokens JWT.
/// La implementacion concreta vive en Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Genera un access token JWT para el usuario indicado.</summary>
    string GenerateAccessToken(Guid userId, string email, string displayName, IEnumerable<string> roles);

    /// <summary>Genera un refresh token opaco criptograficamente seguro.</summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Extrae el UserId del access token aunque este expirado.
    /// Devuelve null si el token es invalido o mal formado.
    /// </summary>
    Guid? GetUserIdFromExpiredToken(string accessToken);

    /// <summary>Tiempo de vida del access token en minutos.</summary>
    int AccessTokenLifetimeMinutes { get; }

    /// <summary>Tiempo de vida del refresh token en dias.</summary>
    int RefreshTokenLifetimeDays { get; }
}
