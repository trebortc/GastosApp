using GastosApp.Application.Reports;

namespace GastosApp.Mobile.Services;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync(DateTimeOffset from, DateTimeOffset to,
        string currency = "MXN", CancellationToken ct = default);
}
