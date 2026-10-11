using Microsoft.AspNetCore.Identity;

namespace GastosApp.Infrastructure.Identity;

/// <summary>
/// Usuario de la aplicacion. Extiende IdentityUser con campos de perfil propios.
/// Vive en Infrastructure para mantener el dominio libre de dependencias de ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string PreferredCurrency { get; set; } = "MXN";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;

    /// <summary>Tokens de actualizacion asociados a este usuario.</summary>
    public ICollection<ApplicationRefreshToken> RefreshTokens { get; set; } = [];
}
