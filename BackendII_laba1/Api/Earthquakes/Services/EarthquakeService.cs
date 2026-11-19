using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seismic.CatalogService.Common;
using Seismic.CatalogService.Configuration;
using Seismic.CatalogService.DataAccess;
using Seismic.CatalogService.DataAccess.Entities;

namespace Seismic.CatalogService.Api.Earthquakes.Services;

public class EarthquakeService : IEarthquakeService
{
    private readonly CatalogDbContext _db;
    private readonly MatchingOptions _options;
    private readonly IOptionsMonitor<SimulationOptions> _simulation;
    private readonly ILogger<EarthquakeService> _logger;

    public EarthquakeService(
        CatalogDbContext db,
        IOptions<MatchingOptions> options,
        IOptionsMonitor<SimulationOptions> simulation,
        ILogger<EarthquakeService> logger)
    {
        _db = db;
        _options = options.Value;
        _simulation = simulation;
        _logger = logger;
    }

    public async Task<EarthquakeMatch?> MatchAsync(
        double latitude, double longitude, DateTime feltAtUtc, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var outcome = "error";
        var matched = false;

        try
        {
            if (_options.ArtificialDelayMs > 0) await Task.Delay(_options.ArtificialDelayMs, ct);

            var from = feltAtUtc.AddMinutes(-_options.TimeWindowBeforeMinutes);
            var to = feltAtUtc.AddMinutes(_options.TimeWindowAfterMinutes);

            // Шаг 1 временной фильтр в БД
            var candidates = await FindCandidatesAsync(from, to, ct);

            // Шаг 2 пространственный фильтр в памяти
            var best = await SelectNearestAsync(latitude, longitude, candidates, ct);

            matched = best is not null;
            outcome = "ok";
            return best;
        }
        finally
        {
            CatalogTelemetry.MatchDuration.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("outcome", outcome),
                new KeyValuePair<string, object?>("matched", matched));
        }
    }

    // выбор кандидатов по времени
    private async Task<List<Earthquake>> FindCandidatesAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        using var activity = CatalogTelemetry.Source.StartActivity("catalog.find-candidates");
        activity?.SetTag("seismic.window.from", from.ToString("O"));
        activity?.SetTag("seismic.window.to", to.ToString("O"));

        var candidates = await _db.Earthquakes
            .Include(e => e.NearestStation)
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to)
            .ToListAsync(ct);

        activity?.SetTag("seismic.candidates.count", candidates.Count);
        _logger.LogInformation(
            "Найдено кандидатов по времени: {CandidatesCount} (окно {From:O} — {To:O})",
            candidates.Count, from, to);

        return candidates;
    }

    // выбор ближайшего эпицентра
    private async Task<EarthquakeMatch?> SelectNearestAsync(
        double latitude, double longitude, List<Earthquake> candidates, CancellationToken ct)
    {
        using var activity = CatalogTelemetry.Source.StartActivity("catalog.select-nearest");
        activity?.SetTag("seismic.max_distance_km", _options.MaxEpicenterDistanceKm);
        activity?.SetTag("seismic.candidates.count", candidates.Count);

        try
        {
            await SimulateProblemsAsync(activity, ct);

            EarthquakeMatch? best = null;
            foreach (var e in candidates)
            {
                var distance = GeoDistance.HaversineKm(latitude, longitude, e.EpicenterLat, e.EpicenterLon);
                if (distance > _options.MaxEpicenterDistanceKm)
                {
                    continue;
                }

                if (best is null || distance < best.DistanceToEpicenterKm)
                {
                    best = new EarthquakeMatch(e, distance);
                }
            }

            activity?.SetTag("seismic.matched", best is not null);
            if (best is not null)
            {
                activity?.SetTag("seismic.event.id", best.Earthquake.Id);
                activity?.SetTag("seismic.event.magnitude", best.Earthquake.Magnitude);
                activity?.SetTag("seismic.distance_km", Math.Round(best.DistanceToEpicenterKm, 1));
            }

            return best;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("seismic.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            activity.RecordFailure(ex); 
            throw;
        }
    }

    private async Task SimulateProblemsAsync(Activity? activity, CancellationToken ct)
    {
        var sim = _simulation.CurrentValue;

        if (Random.Shared.NextDouble() < sim.SlowProbability)
        {
            activity?.SetTag("seismic.simulation.slow", true);
            activity?.SetTag("seismic.simulation.delay_ms", sim.SlowDelayMs);
            CatalogTelemetry.SimulationEvents.Add(1, new KeyValuePair<string, object?>("kind", "slow"));
            _logger.LogWarning("Имитация медленной операции: задержка {DelayMs} мс", sim.SlowDelayMs);

            await Task.Delay(sim.SlowDelayMs, ct);
        }

        if (Random.Shared.NextDouble() < sim.ErrorProbability)
        {
            activity?.SetTag("seismic.simulation.error", true);
            CatalogTelemetry.SimulationEvents.Add(1, new KeyValuePair<string, object?>("kind", "error"));

            throw new SimulatedCatalogFailureException(
                "Имитация сбоя при выборе ближайшего эпицентра (Simulation:ErrorProbability).");
        }
    }

    public IQueryable<Earthquake> GetQueryable() => _db.Earthquakes.AsNoTracking();
}
