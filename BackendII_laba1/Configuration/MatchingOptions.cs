namespace Seismic.CatalogService.Configuration;

public class MatchingOptions
{
    public double MaxEpicenterDistanceKm { get; set; } = 300;
    public int TimeWindowBeforeMinutes { get; set; } = 60;
    public int TimeWindowAfterMinutes { get; set; } = 5;
    public int ArtificialDelayMs { get; set; } = 0;
}