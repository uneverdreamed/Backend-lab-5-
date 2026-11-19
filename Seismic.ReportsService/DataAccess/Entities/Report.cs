namespace Seismic.ReportsService.DataAccess.Entities;

public class Report
{
    public long Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
    public int FeltIntensity { get; set; }
    public DateTime FeltAt { get; set; }

    public string? Comment { get; set; }

    public long? EarthquakeId { get; set; }

    public bool IsMatched { get; set; }

    public DateTime? MatchedAt { get; set; }

    public double? DistanceToEpicenterKm { get; set; }
    public string? RegionName { get; set; }
    public DateTime CreatedAt { get; set; }
}
