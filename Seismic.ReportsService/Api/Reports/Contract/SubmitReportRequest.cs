using System.ComponentModel.DataAnnotations;

namespace Seismic.ReportsService.Api.Reports.Contract;

public class SubmitReportRequest
{
    [Range(-90, 90, ErrorMessage = "Широта должна быть в диапазоне [-90; 90].")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Долгота должна быть в диапазоне [-180; 180].")]
    public double Longitude { get; set; }

    [Range(1, 10, ErrorMessage = "Интенсивность должна быть в диапазоне [1; 10].")]
    public int FeltIntensity { get; set; }
    public DateTime? FeltAt { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
