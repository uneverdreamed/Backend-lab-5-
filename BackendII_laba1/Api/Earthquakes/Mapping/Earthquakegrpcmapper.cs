using Google.Protobuf.WellKnownTypes;
using Seismic.CatalogService.DataAccess.Entities;
using Seismic.CatalogService.Grpc;

namespace Seismic.CatalogService.Api.Earthquakes.Mapping;

// вручную, так как происходит конвертация DateTime в Timestamp и вложенное сообщение о станции
public static class EarthquakeGrpcMapper
{
    public static EarthquakeMessage ToMessage(Earthquake e)
    {
        var message = new EarthquakeMessage
        {
            Id = e.Id,
            ExternalId = e.ExternalId,
            EpicenterLat = e.EpicenterLat,
            EpicenterLon = e.EpicenterLon,
            Depth = e.Depth,
            Magnitude = e.Magnitude,
            OccurredAt = Timestamp.FromDateTime(DateTime.SpecifyKind(e.OccurredAt, DateTimeKind.Utc)),
            RegionName = e.RegionName ?? string.Empty,
            Source = e.Source
        };

        if (e.NearestStation is not null)
        {
            message.NearestStation = new NearestStation
            {
                Code = e.NearestStation.Code,
                Name = e.NearestStation.Name,
                Organization = e.NearestStation.Organization ?? string.Empty,
                DistanceToStationKm = e.DistanceToStationKm ?? 0
            };
        }

        return message;
    }
}