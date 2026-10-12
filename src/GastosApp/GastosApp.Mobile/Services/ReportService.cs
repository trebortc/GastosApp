using System.Net.Http.Json;
using System.Web;
using GastosApp.Application.Reports;

namespace GastosApp.Mobile.Services;

public sealed class ReportService : IReportService
{
    private readonly HttpClient _http;

    public ReportService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GastosApi");
    }

    public async Task<DashboardDto> GetDashboardAsync(
        DateTimeOffset from, DateTimeOffset to,
        string currency = "MXN", CancellationToken ct = default)
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        q["from"]     = from.ToString("O");
        q["to"]       = to.ToString("O");
        q["currency"] = currency;

        var url = $"api/v1/reports/dashboard?{q}";
        var result = await _http.GetFromJsonAsync<DashboardDto>(url, ct);
        return result ?? new DashboardDto(0, 0, 0, currency, from, to, [], []);
    }
}
