using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Seismic.ReportsService.Common;
public static class ReportsTelemetry
{
    public const string ServiceName = "seismic-reports";
    public const string SourceName = "Seismic.ReportsService";

    public static readonly ActivitySource Source = new(SourceName);
    public static readonly Meter Meter = new(SourceName);

    public static readonly Counter<long> ReportsSubmitted = Meter.CreateCounter<long>(
        "seismic.reports.submitted",
        description: "Количество обработанных отчётов о землетрясениях");

    public static readonly Histogram<double> SubmitDuration = Meter.CreateHistogram<double>(
        "seismic.reports.submit.duration",
        unit: "ms",
        description: "Длительность обработки отчёта");
}
