namespace Seismic.CatalogService.DataAccess.Entities;

public class SeismicStation
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Region { get; set; }
    public string? Organization { get; set; }
    public ICollection<Earthquake> Earthquakes { get; set; } = new List<Earthquake>();
}