using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Seismic.CatalogService.Grpc;
using Seismic.ReportsService.Api.Reports.Contract;

namespace Seismic.ReportsService.External.GrpcClients;

public class EventCatalogClient : IEventCatalogClient
{
    private readonly EventCatalog.EventCatalogClient _client;
    private readonly int _timeoutSeconds;
    private readonly ILogger<EventCatalogClient> _logger;

    public EventCatalogClient(
        EventCatalog.EventCatalogClient client,
        IConfiguration configuration,
        ILogger<EventCatalogClient> logger)
    {
        _client = client;
        _timeoutSeconds = configuration.GetValue("CatalogService:TimeoutSeconds", 2);
        _logger = logger;
    }

    public async Task<CatalogMatchResult> MatchAsync(
        double latitude, double longitude, DateTime feltAtUtc, CancellationToken ct)
    {
        var request = new MatchEventRequest
        {
            Latitude = latitude,
            Longitude = longitude,
            FeltAt = Timestamp.FromDateTime(DateTime.SpecifyKind(feltAtUtc, DateTimeKind.Utc))
        };

        try
        {
            // по истечении дедлайна gRPC сам прерывает вызов со статусом DeadlineExceeded
            var response = await _client.MatchEventAsync(
                request,
                deadline: DateTime.UtcNow.AddSeconds(_timeoutSeconds),
                cancellationToken: ct);

            if (!response.Matched)
            {
                return new CatalogMatchResult(false, null);
            }

            var e = response.Earthquake;
            var info = new MatchedEarthquakeInfo
            {
                Id = e.Id,
                ExternalId = e.ExternalId,
                Magnitude = e.Magnitude,
                Depth = e.Depth,
                RegionName = e.RegionName,
                OccurredAt = e.OccurredAt.ToDateTime(),
                DistanceToEpicenterKm = response.DistanceToEpicenterKm,
                NearestStation = e.NearestStation is null
                    ? null
                    : new NearestStationInfo
                    {
                        Code = e.NearestStation.Code,
                        Name = e.NearestStation.Name,
                        Organization = e.NearestStation.Organization,
                        DistanceToStationKm = e.NearestStation.DistanceToStationKm
                    }
            };

            return new CatalogMatchResult(true, info);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _logger.LogWarning("Каталог не ответил за {Timeout} с (DeadlineExceeded).", _timeoutSeconds);
            throw new CatalogTimeoutException(
                $"Каталог не ответил в течение {_timeoutSeconds} с.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            _logger.LogWarning("Каталог недоступен (Unavailable): {Detail}", ex.Status.Detail);
            throw new CatalogUnavailableException("Сервис каталога событий недоступен.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            _logger.LogWarning("Каталог отклонил запрос (InvalidArgument): {Detail}", ex.Status.Detail);
            throw new CatalogValidationException(ex.Status.Detail);
        }
        catch (RpcException ex)
        {
            // Любой другой статус (в первую очередь Internal — сбой внутри каталога)
            _logger.LogError(ex, "Каталог вернул ошибку {StatusCode}: {Detail}", ex.StatusCode, ex.Status.Detail);
            throw new CatalogFailureException($"Каталог событий вернул ошибку ({ex.StatusCode}).");
        }
    }
}
