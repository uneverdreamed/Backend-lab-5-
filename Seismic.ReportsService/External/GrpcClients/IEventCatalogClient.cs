using Seismic.ReportsService.Api.Reports.Contract;

namespace Seismic.ReportsService.External.GrpcClients;

// было ли совпадение и данные события
public record CatalogMatchResult(bool Matched, MatchedEarthquakeInfo? Earthquake);

public interface IEventCatalogClient
{
    // обращение к каталогу по gRPC
    // CatalogUnavailableException/CatalogTimeoutException/CatalogValidationException при соответствующих сбоях
    Task<CatalogMatchResult> MatchAsync(double latitude, double longitude, DateTime feltAtUtc, CancellationToken ct);
}
