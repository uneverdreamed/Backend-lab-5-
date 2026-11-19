namespace Seismic.CatalogService.Api.Earthquakes.Services;

// Исключение, которым имитируется сбой внутри каталога (см. Simulation:ErrorProbability)
public class SimulatedCatalogFailureException : Exception
{
    public SimulatedCatalogFailureException(string message) : base(message) { }
}
