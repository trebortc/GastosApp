using GastosApp.Application.Authentication;
using GastosApp.Domain.Entities;
using GastosApp.Infrastructure.Identity;
using GastosApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IJwtTokenService jwt)
    {
        _userManager = userManager;
        _db = db;
        _jwt = jwt;
    }

    /// <summary>Registra un nuevo usuario.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.PreferredCurrency) || request.PreferredCurrency.Length != 3)
            return BadRequest(new { error = "La moneda preferida debe ser un codigo ISO 4217 de tres letras." });

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { error = "Ya existe una cuenta con ese correo electronico." });

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            DisplayName = request.DisplayName.Trim(),
            PreferredCurrency = request.PreferredCurrency.ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // Crear perfil de dominio
        var profile = UserProfile.Create(user.Id, user.DisplayName, user.PreferredCurrency);
        await _db.UserProfiles.AddAsync(profile, ct);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, new { message = "Usuario registrado correctamente.", userId = user.Id });
    }

    /// <summary>Inicia sesion y devuelve access token + refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { error = "Credenciales invalidas." });

        if (!user.IsActive)
            return Unauthorized(new { error = "La cuenta esta desactivada." });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(await IssueTokensAsync(user, roles, ct));
    }

    /// <summary>Renueva el access token usando un refresh token valido.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _jwt.GetUserIdFromExpiredToken(request.AccessToken);
        if (userId is null)
            return Unauthorized(new { error = "Access token invalido." });

        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == request.RefreshToken &&
                t.UserId == userId &&
                !t.IsRevoked &&
                t.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (stored is null)
            return Unauthorized(new { error = "Refresh token invalido o expirado." });

        // Revocar el token usado (rotacion de tokens)
        stored.IsRevoked = true;
        stored.RevokedAt = DateTimeOffset.UtcNow;

        var roles = await _userManager.GetRolesAsync(stored.User);
        var response = await IssueTokensAsync(stored.User, roles, ct);
        return Ok(response);
    }

    /// <summary>Revoca el refresh token y cierra la sesion.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.Token == request.RefreshToken &&
                t.UserId == userId &&
                !t.IsRevoked, ct);

        if (token is not null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    /// <summary>Devuelve el perfil del usuario autenticado.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return NotFound();

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            preferredCurrency = user.PreferredCurrency,
            createdAt = user.CreatedAt
        });
    }

    // -- Helpers --

    private async Task<AuthResponse> IssueTokensAsync(
        ApplicationUser user, IList<string> roles, CancellationToken ct)
    {
        var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email!, user.DisplayName, roles);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        var refreshToken = new ApplicationRefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenLifetimeDays),
            CreatedAt = DateTimeOffset.UtcNow,
            DeviceInfo = Request.Headers.UserAgent.ToString()
        };

        await _db.RefreshTokens.AddAsync(refreshToken, ct);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(_jwt.AccessTokenLifetimeMinutes),
            UserId: user.Id,
            DisplayName: user.DisplayName,
            Email: user.Email!);
    }
}
