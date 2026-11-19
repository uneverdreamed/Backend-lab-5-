using System.Diagnostics;
using Grpc.Core;
using Seismic.CatalogService.Api.Earthquakes.Mapping;
using Seismic.CatalogService.Api.Earthquakes.Services;
using Seismic.CatalogService.Common;
using Seismic.CatalogService.Grpc;

namespace Seismic.CatalogService.Api.Earthquakes.Grpc;

public class EventCatalogGrpcHandler : EventCatalog.EventCatalogBase
{
    private readonly IEarthquakeService _service;
    private readonly ILogger<EventCatalogGrpcHandler> _logger;

    public EventCatalogGrpcHandler(IEarthquakeService service, ILogger<EventCatalogGrpcHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public override async Task<MatchEventResponse> MatchEvent(MatchEventRequest request, ServerCallContext context)
    {
        if (request.Latitude is < -90 or > 90)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                "Широта должна быть в диапазоне [-90; 90]."));
        }

        if (request.Longitude is < -180 or > 180)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                "Долгота должна быть в диапазоне [-180; 180]."));
        }

        if (request.FeltAt is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                "Не указан момент времени felt_at."));
        }

        var feltAtUtc = request.FeltAt.ToDateTime();

        _logger.LogInformation(
            "Получен запрос MatchEvent: ({Lat}; {Lon}), момент {FeltAt:O}",
            request.Latitude, request.Longitude, feltAtUtc);

        EarthquakeMatch? match;
        try
        {
            match = await _service.MatchAsync(
                request.Latitude, request.Longitude, feltAtUtc, context.CancellationToken);
        }
        catch (SimulatedCatalogFailureException ex)
        {
            _logger.LogError(ex,
                "Сопоставление для ({Lat}; {Lon}) завершилось ошибкой",
                request.Latitude, request.Longitude);

            Activity.Current.RecordFailure(ex);

            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка каталога событий."));
        }

        if (match is null)
        {
            _logger.LogInformation(
                "Совпадений не найдено для ({Lat}; {Lon}) в момент {FeltAt:O}",
                request.Latitude, request.Longitude, feltAtUtc);
            return new MatchEventResponse { Matched = false };
        }

        _logger.LogInformation(
            "Отчёт сопоставлен с событием {ExternalId} (M{Mag}), расстояние {Dist:F1} км",
            match.Earthquake.ExternalId, match.Earthquake.Magnitude, match.DistanceToEpicenterKm);

        return new MatchEventResponse
        {
            Matched = true,
            Earthquake = EarthquakeGrpcMapper.ToMessage(match.Earthquake),
            DistanceToEpicenterKm = match.DistanceToEpicenterKm
        };
    }
}
