using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Seismic.CatalogService.Grpc;
using Seismic.ReportsService;
using Seismic.ReportsService.Api.Reports.Services;
using Seismic.ReportsService.Common;
using Seismic.ReportsService.DataAccess;
using Seismic.ReportsService.External.GrpcClients;

// Разрешаем gRPC поверх plaintext HTTP/2 (каталог слушает http://, без TLS).
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Логи: Serilog пишет в консоль, а через writeToProviders те же события получает
// провайдер OpenTelemetry и отправляет в Collector по OTLP.
builder.Logging.ClearProviders();
builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration), writeToProviders: true);

// Телеметрия: три сигнала (логи, трейсы, метрики) уходят в OpenTelemetry Collector по OTLP.
// Адрес по умолчанию — http://localhost:4317 (gRPC); меняется переменной OTEL_EXPORTER_OTLP_ENDPOINT.
builder.Services.AddOpenTelemetry()
    // service.name — имя приложения в Grafana; атрибут student попадает в resource каждого спана
    .ConfigureResource(resource => resource
        .AddService(ReportsTelemetry.ServiceName)
        .AddAttributes(new Dictionary<string, object> { ["student"] = MeInfo.INFOLINE }))
    .WithLogging()
    .WithTracing(tracing => tracing
        .AddSource(ReportsTelemetry.SourceName)                          // собственные спаны
        .AddAspNetCoreInstrumentation(o => o.RecordException = true)    // серверный спан POST /reports
        .AddGrpcClientInstrumentation()                                  // клиентский спан gRPC-вызова каталога;
                                                                         // он же добавляет заголовок traceparent
        .AddHttpClientInstrumentation()                                  // HTTP/2-запрос, на котором работает gRPC
        .AddEntityFrameworkCoreInstrumentation())                        // спаны запросов к SQLite
    .WithMetrics(metrics => metrics
        .AddMeter(ReportsTelemetry.SourceName)                           // собственные метрики
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .SetExemplarFilter(ExemplarFilterType.TraceBased))               // exemplars: переход от точки графика к трейсу
    .UseOtlpExporter();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Reports")));

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IEventCatalogClient, EventCatalogClient>();

// Регистрируем типизированный gRPC-клиент каталога; адрес берётся из конфигурации.
builder.Services.AddGrpcClient<EventCatalog.EventCatalogClient>(options =>
{
    options.Address = new Uri(builder.Configuration["CatalogService:BaseUrl"]!);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    db.Database.EnsureCreated();
}

PrintStartupBanner();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

void PrintStartupBanner()
{
    Console.WriteLine("========================================================");
    Console.WriteLine(" Сервис отчётов о сейсмических событиях (HTTP API)");
    Console.WriteLine($" {MeInfo.INFOLINE}");
    Console.WriteLine("========================================================");
}
