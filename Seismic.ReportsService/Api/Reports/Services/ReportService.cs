using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Seismic.ReportsService.Api.Reports.Contract;
using Seismic.ReportsService.Common;
using Seismic.ReportsService.DataAccess;
using Seismic.ReportsService.DataAccess.Entities;
using Seismic.ReportsService.External.GrpcClients;

namespace Seismic.ReportsService.Api.Reports.Services;

public class ReportService : IReportService
{
    private readonly ReportsDbContext _db;
    private readonly IEventCatalogClient _catalog;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ReportsDbContext db, IEventCatalogClient catalog, ILogger<ReportService> logger)
    {
        _db = db;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<ReportResult> SubmitAsync(SubmitReportRequest request, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var outcome = "failed";

        try
        {
            var feltAt = (request.FeltAt ?? DateTime.UtcNow).ToUniversalTime();

            _logger.LogInformation(
                "Принят отчёт: ({Lat}; {Lon}), интенсивность {Intensity}, момент {FeltAt:O}",
                request.Latitude, request.Longitude, request.FeltIntensity, feltAt);

            var match = await EnrichWithEventAsync(request, feltAt, ct);
            var report = await SaveReportAsync(request, feltAt, match, ct);

            outcome = match.Matched ? "matched" : "unmatched";

            _logger.LogInformation(
                "Отчёт {ReportId} сохранён: сопоставлен = {Matched}, событие {EarthquakeId}",
                report.Id, match.Matched, report.EarthquakeId);

            return new ReportResult
            {
                ReportId = report.Id,
                Matched = match.Matched,
                Message = match.Matched
                    ? "Отчёт принят и сопоставлен с зарегистрированным событием."
                    : "Отчёт принят. Подходящее событие пока не найдено — он будет сопоставлен позже.",
                Earthquake = match.Earthquake,
                ProcessedBy = MeInfo.INFOLINE
            };
        }
        catch (Exception ex)
        {
            // Лог с исключением уходит в Loki вместе с TraceId — по нему находим упавший трейс
            _logger.LogError(ex,
                "Не удалось обработать отчёт ({Lat}; {Lon}): {Reason}",
                request.Latitude, request.Longitude, ex.Message);
            throw;
        }
        finally
        {
            // Собственные метрики: счётчик по исходам и гистограмма длительности
            ReportsTelemetry.ReportsSubmitted.Add(1,
                new KeyValuePair<string, object?>("outcome", outcome));
            ReportsTelemetry.SubmitDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("outcome", outcome));
        }
    }

    // Собственный спан №1: обогащение отчёта данными каталога (внутри — клиентский спан gRPC-вызова)
    private async Task<CatalogMatchResult> EnrichWithEventAsync(
        SubmitReportRequest request, DateTime feltAt, CancellationToken ct)
    {
        using var activity = ReportsTelemetry.Source.StartActivity("report.enrich-with-event");
        activity?.SetTag("seismic.report.latitude", request.Latitude);
        activity?.SetTag("seismic.report.longitude", request.Longitude);
        activity?.SetTag("seismic.report.felt_intensity", request.FeltIntensity);
        activity?.SetTag("seismic.report.felt_at", feltAt.ToString("O"));

        try
        {
            var match = await _catalog.MatchAsync(request.Latitude, request.Longitude, feltAt, ct);

            activity?.SetTag("seismic.matched", match.Matched);
            if (match.Matched && match.Earthquake is not null)
            {
                activity?.SetTag("seismic.event.id", match.Earthquake.Id);
                activity?.SetTag("seismic.event.magnitude", match.Earthquake.Magnitude);
                activity?.SetTag("seismic.distance_km", Math.Round(match.Earthquake.DistanceToEpicenterKm, 1));
            }

            return match;
        }
        catch (Exception ex)
        {
            activity.RecordFailure(ex); // статус Error + событие exception
            throw;
        }
    }

    // Собственный спан №2: сохранение отчёта (внутри — спан INSERT от инструментации EF Core)
    private async Task<Report> SaveReportAsync(
        SubmitReportRequest request, DateTime feltAt, CatalogMatchResult match, CancellationToken ct)
    {
        using var activity = ReportsTelemetry.Source.StartActivity("report.save");

        try
        {
            var report = new Report
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                FeltIntensity = request.FeltIntensity,
                FeltAt = feltAt,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                IsMatched = match.Matched
            };

            if (match.Matched && match.Earthquake is not null)
            {
                report.EarthquakeId = match.Earthquake.Id;
                report.RegionName = match.Earthquake.RegionName;
                report.DistanceToEpicenterKm = match.Earthquake.DistanceToEpicenterKm;
                report.MatchedAt = DateTime.UtcNow;
            }

            _db.Reports.Add(report);
            await _db.SaveChangesAsync(ct);

            activity?.SetTag("seismic.report.id", report.Id);
            activity?.SetTag("seismic.matched", report.IsMatched);
            activity?.SetTag("seismic.report.region", report.RegionName);

            return report;
        }
        catch (Exception ex)
        {
            activity.RecordFailure(ex);
            throw;
        }
    }

    public Task<List<Report>> GetAllAsync() =>
        _db.Reports.AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync();

    public Task<Report?> GetByIdAsync(long id) =>
        _db.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
}
