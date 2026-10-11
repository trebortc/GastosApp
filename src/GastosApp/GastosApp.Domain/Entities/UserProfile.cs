namespace GastosApp.Domain.Entities;

/// <summary>
/// Referencia ligera al usuario dentro del dominio.
/// La entidad de identidad completa (ApplicationUser : IdentityUser) vive en Infrastructure
/// para mantener el dominio libre de dependencias de ASP.NET Core Identity.
/// </summary>
public sealed class UserProfile
{
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string PreferredCurrency { get; private set; } = "MXN";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(Guid userId, string displayName, string preferredCurrency = "MXN")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El identificador de usuario no puede ser vacío.", nameof(userId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("El nombre de perfil es obligatorio.", nameof(displayName));

        if (string.IsNullOrWhiteSpace(preferredCurrency) || preferredCurrency.Length != 3)
            throw new ArgumentException("La moneda preferida debe ser un código ISO 4217 de tres letras.", nameof(preferredCurrency));

        var now = DateTimeOffset.UtcNow;
        return new UserProfile
        {
            UserId = userId,
            DisplayName = displayName.Trim(),
            PreferredCurrency = preferredCurrency.ToUpperInvariant(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateProfile(string displayName, string? avatarUrl, string preferredCurrency)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("El nombre de perfil es obligatorio.", nameof(displayName));

        if (string.IsNullOrWhiteSpace(preferredCurrency) || preferredCurrency.Length != 3)
            throw new ArgumentException("La moneda preferida debe ser un código ISO 4217 de tres letras.", nameof(preferredCurrency));

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        PreferredCurrency = preferredCurrency.ToUpperInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
