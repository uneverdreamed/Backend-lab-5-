namespace Seismic.CatalogService.DataAccess.Entities;

public class Earthquake
{
    public long Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;

    public double EpicenterLat { get; set; }

    public double EpicenterLon { get; set; }
    public double Depth { get; set; }
    public double Magnitude { get; set; }
    public DateTime OccurredAt { get; set; }

    public string? RegionName { get; set; }

    public string Source { get; set; } = string.Empty;
    public long? NearestStationId { get; set; }

    public SeismicStation? NearestStation { get; set; }
    public double? DistanceToStationKm { get; set; }
}