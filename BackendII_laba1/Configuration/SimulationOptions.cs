namespace Seismic.CatalogService.Configuration;

// Имитация проблем для демонстрации трассировки (лаба 5):
public class SimulationOptions
{
    public double SlowProbability { get; set; } = 0;
    public int SlowDelayMs { get; set; } = 1500;
    public double ErrorProbability { get; set; } = 0;
}
