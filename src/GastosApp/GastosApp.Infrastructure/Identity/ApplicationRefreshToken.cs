namespace GastosApp.Infrastructure.Identity;

/// <summary>
/// Token de actualizacion (refresh token) almacenado en la base de datos.
/// Permite renovar el JWT sin requerir que el usuario vuelva a autenticarse.
/// Los tokens expirados o revocados se conservan para auditoria.
/// </summary>
public sealed class ApplicationRefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRevoked { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }

    public bool IsActive => !IsRevoked && DateTimeOffset.UtcNow < ExpiresAt;

    public ApplicationUser User { get; set; } = null!;
}
