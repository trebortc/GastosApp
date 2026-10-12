using System.Net.Http.Json;
using GastosApp.Application.Authentication;

namespace GastosApp.Mobile.Services;

public sealed class AuthService : IAuthService
{
    private const string AccessTokenKey  = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    private readonly HttpClient _http;

    private string? _cachedAccessToken;

    public AuthService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GastosApi");
    }

    public bool IsAuthenticated => _cachedAccessToken is not null;

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_cachedAccessToken is not null)
            return _cachedAccessToken;

        _cachedAccessToken = await SecureStorage.GetAsync(AccessTokenKey);
        return _cachedAccessToken;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/auth/login",
            new LoginRequest(email, password), ct);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Respuesta vacia del servidor.");

        await PersistTokensAsync(auth);
        return auth;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password,
        string displayName, string currency = "MXN", CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/auth/register",
            new RegisterRequest(email, password, displayName, currency), ct);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Respuesta vacia del servidor.");

        await PersistTokensAsync(auth);
        return auth;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (refreshToken is not null)
            {
                await _http.PostAsJsonAsync("api/v1/auth/logout",
                    new LogoutRequest(refreshToken), ct);
            }
        }
        catch { /* no bloquear logout local si la API no responde */ }
        finally
        {
            _cachedAccessToken = null;
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
        }
    }

    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        var accessToken  = await SecureStorage.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);

        if (accessToken is null || refreshToken is null)
            return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/refresh",
                new RefreshTokenRequest(accessToken, refreshToken), ct);

            if (!response.IsSuccessStatusCode)
                return false;

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
            if (auth is null) return false;

            await PersistTokensAsync(auth);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task PersistTokensAsync(AuthResponse auth)
    {
        _cachedAccessToken = auth.AccessToken;
        await SecureStorage.SetAsync(AccessTokenKey, auth.AccessToken);
        await SecureStorage.SetAsync(RefreshTokenKey, auth.RefreshToken);
    }
}
