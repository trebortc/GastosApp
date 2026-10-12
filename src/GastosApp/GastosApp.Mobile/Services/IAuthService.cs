using GastosApp.Application.Authentication;

namespace GastosApp.Mobile.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResponse> RegisterAsync(string email, string password, string displayName, string currency = "MXN", CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> TryRefreshAsync(CancellationToken ct = default);
    Task<string?> GetAccessTokenAsync();
    bool IsAuthenticated { get; }
}
