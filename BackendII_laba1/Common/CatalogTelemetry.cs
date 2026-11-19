using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Seismic.CatalogService.Common;
public static class CatalogTelemetry
{
    public const string ServiceName = "seismic-catalog";
    public const string SourceName = "Seismic.CatalogService";

    public static readonly ActivitySource Source = new(SourceName);
    public static readonly Meter Meter = new(SourceName);
    public static readonly Histogram<double> MatchDuration = Meter.CreateHistogram<double>(
        "seismic.catalog.match.duration",
        unit: "ms",
        description: "Длительность сопоставления отчёта с событием в каталоге");
    public static readonly Counter<long> SimulationEvents = Meter.CreateCounter<long>(
        "seismic.catalog.simulation.events",
        description: "Сколько раз сработала имитация медленной операции или ошибки");
}
