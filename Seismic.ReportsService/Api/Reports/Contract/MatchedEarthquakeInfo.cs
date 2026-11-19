namespace Seismic.ReportsService.Api.Reports.Contract;

public class MatchedEarthquakeInfo
{
    public long Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public double Magnitude { get; set; }
    public double Depth { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public double DistanceToEpicenterKm { get; set; }
    public NearestStationInfo? NearestStation { get; set; }
}
