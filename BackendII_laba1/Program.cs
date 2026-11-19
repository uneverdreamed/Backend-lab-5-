using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Seismic.CatalogService;
using Seismic.CatalogService.Api.Earthquakes.GraphQL;
using Seismic.CatalogService.Api.Earthquakes.Grpc;
using Seismic.CatalogService.Api.Earthquakes.Services;
using Seismic.CatalogService.Common;
using Seismic.CatalogService.Configuration;
using Seismic.CatalogService.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration), writeToProviders: true);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(CatalogTelemetry.ServiceName)
        .AddAttributes(new Dictionary<string, object> { ["student"] = MeInfo.INFOLINE }))
    .WithLogging()
    .WithTracing(tracing => tracing
        .AddSource(CatalogTelemetry.SourceName)                         
        .AddAspNetCoreInstrumentation(o => o.RecordException = true)   
        .AddEntityFrameworkCoreInstrumentation())                       
    .WithMetrics(metrics => metrics
        .AddMeter(CatalogTelemetry.SourceName)                     
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .SetExemplarFilter(ExemplarFilterType.TraceBased))            
    .UseOtlpExporter();

builder.Services.AddGrpc();

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Catalog")));

builder.Services.Configure<MatchingOptions>(builder.Configuration.GetSection("Matching"));
builder.Services.Configure<SimulationOptions>(builder.Configuration.GetSection("Simulation"));
builder.Services.AddScoped<IEarthquakeService, EarthquakeService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.EnsureCreated();
}

PrintStartupBanner();

app.MapGrpcService<EventCatalogGrpcHandler>();
app.MapGraphQL();

app.Run();

void PrintStartupBanner()
{
    Console.WriteLine("========================================================");
    Console.WriteLine(" Сервис каталога сейсмических событий (gRPC-сервер)");
    Console.WriteLine($" {MeInfo.INFOLINE}");
    Console.WriteLine("========================================================");
}
