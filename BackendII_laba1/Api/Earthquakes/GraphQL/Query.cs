using HotChocolate;
using HotChocolate.Data;
using Seismic.CatalogService.Api.Earthquakes.Contract;
using Seismic.CatalogService.Api.Earthquakes.Services;

namespace Seismic.CatalogService.Api.Earthquakes.GraphQL;

public class Query
{
    public string About => MeInfo.INFOLINE;

    [UseProjection] 
    [UseFiltering] 
    [UseSorting] 
    public IQueryable<EarthquakeResponse> GetEarthquakes([Service] IEarthquakeService service)
    {
        return service.GetQueryable().Select(e => new EarthquakeResponse
        {
            Id = e.Id,
            ExternalId = e.ExternalId,
            EpicenterLat = e.EpicenterLat,
            EpicenterLon = e.EpicenterLon,
            Depth = e.Depth,
            Magnitude = e.Magnitude,
            OccurredAt = e.OccurredAt,
            RegionName = e.RegionName,
            Source = e.Source,
            NearestStationName = e.NearestStation != null ? e.NearestStation.Name : null,
            DistanceToStationKm = e.DistanceToStationKm
        });
    }
}