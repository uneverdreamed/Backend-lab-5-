namespace Seismic.ReportsService.Api.Reports.Contract;

public class NearestStationInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public double DistanceToStationKm { get; set; }
}
