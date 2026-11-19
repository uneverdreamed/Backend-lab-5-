using Seismic.CatalogService.DataAccess.Entities;

namespace Seismic.CatalogService.Api.Earthquakes.Services;

public record EarthquakeMatch(Earthquake Earthquake, double DistanceToEpicenterKm);

public interface IEarthquakeService
{
    Task<EarthquakeMatch?> MatchAsync(double latitude, double longitude, DateTime feltAtUtc, CancellationToken ct);
    IQueryable<Earthquake> GetQueryable();
}