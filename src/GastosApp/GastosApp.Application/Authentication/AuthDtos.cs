using System.ComponentModel.DataAnnotations;

namespace GastosApp.Application.Authentication;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)]
    string Email,

    [Required, MinLength(8), MaxLength(100)]
    string Password,

    [Required, MinLength(2), MaxLength(100)]
    string DisplayName,

    [Required, StringLength(3, MinimumLength = 3)]
    string PreferredCurrency = "MXN");

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256)]
    string Email,

    [Required]
    string Password);

public sealed record RefreshTokenRequest(
    [Required]
    string AccessToken,

    [Required]
    string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    string DisplayName,
    string Email);

public sealed record LogoutRequest(
    [Required]
    string RefreshToken);
