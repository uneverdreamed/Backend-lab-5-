using Microsoft.AspNetCore.Mvc;
using Seismic.ReportsService.Api.Reports.Contract;
using Seismic.ReportsService.Api.Reports.Services;
using Seismic.ReportsService.External.GrpcClients;

namespace Seismic.ReportsService.Api.Reports.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Прием отчета (валидация входа, вызов каталога по gRPC, сохранение результата)
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitReportRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _reportService.SubmitAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.ReportId }, result);
        }
        catch (CatalogValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest,
                title: "Некорректные данные отчёта");
        }
        catch (CatalogUnavailableException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Каталог недоступен");
        }
        catch (CatalogTimeoutException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status504GatewayTimeout,
                title: "Превышено время ожидания каталога");
        }
        catch (CatalogFailureException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway,
                title: "Ошибка каталога событий");
        }
    }

    // список всех отчетов
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    // отчет по Id    
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var report = await _reportService.GetByIdAsync(id);
        return report is null ? NotFound() : Ok(report);
    }
}
