using System.Net.Http.Headers;

namespace GastosApp.Mobile.Services;

/// <summary>
/// DelegatingHandler que inyecta el Bearer token en cada request
/// y reintenta una vez con refresh si recibe 401.
/// </summary>
public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly Lazy<IAuthService> _auth;

    public AuthTokenHandler(Lazy<IAuthService> auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _auth.Value.GetAccessTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _auth.Value.TryRefreshAsync(ct);
            if (refreshed)
            {
                var newToken = await _auth.Value.GetAccessTokenAsync();
                var retry = await CloneRequestAsync(request);
                if (newToken is not null)
                    retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(retry, ct);
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var h in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        return clone;
    }
}
