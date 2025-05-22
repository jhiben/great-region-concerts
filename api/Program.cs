using Api.Scrapers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IConcertsScraper, KulturfabrikConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, AtelierConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, RockhalConcertsScraper>();

builder.Build().Run();
