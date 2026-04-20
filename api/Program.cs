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

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("scraper", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});

builder.Services.AddSingleton<IConcertsScraper, KulturfabrikConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, AtelierConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, RockhalConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, PhilharmonieConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, NeimensterConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, Casino2000ConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, TrifolionConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, TufaTrierConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, EuropahalleConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, CiteMusicaleSconcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, OpderschmelzConcertsScraper>();
builder.Services.AddSingleton<IConcertsScraper, ReflektorConcertsScraper>();

builder.Build().Run();
