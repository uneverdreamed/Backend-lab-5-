namespace Seismic.ReportsService.Api.Reports.Contract;

public class ReportResult
{
    public long ReportId { get; set; }

    public bool Matched { get; set; }

    public string Message { get; set; } = string.Empty;
    public MatchedEarthquakeInfo? Earthquake { get; set; } // заполнено, если отчет удалось сопоставить с событием
    public string ProcessedBy { get; set; } = string.Empty;
}
