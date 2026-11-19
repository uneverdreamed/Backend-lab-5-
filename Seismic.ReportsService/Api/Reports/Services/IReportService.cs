using Seismic.ReportsService.Api.Reports.Contract;
using Seismic.ReportsService.DataAccess.Entities;

namespace Seismic.ReportsService.Api.Reports.Services;

public interface IReportService
{
    Task<ReportResult> SubmitAsync(SubmitReportRequest request, CancellationToken ct);

    Task<List<Report>> GetAllAsync();

    Task<Report?> GetByIdAsync(long id);
}
