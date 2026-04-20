using Api.Scrapers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Api;

public class GetConcerts(
    ILogger<GetConcerts> logger,
    IEnumerable<IConcertsScraper> concertsScrapers,
    IMemoryCache memoryCache
)
{
    private const string CacheKey = "concerts";

    [Function("GetConcerts")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "concerts")] HttpRequest req
    )
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        if (memoryCache.TryGetValue(CacheKey, out object? cachedResult))
        {
            logger.LogInformation("Returning cached concert data.");
            return new OkObjectResult(cachedResult);
        }

        var tasks = concertsScrapers
            .Select(async scraper =>
            {
                try
                {
                    return await scraper.FetchConcerts();
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Scraper {Scraper} failed.",
                        scraper.GetType().Name
                    );
                    return Enumerable.Empty<Models.Concert>();
                }
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        var payload = results
            .SelectMany(c => c)
            .GroupBy(c => c.Date)
            .Select(g => new
            {
                Date = g.Key,
                Concerts = g.Select(c => new
                {
                    c.Band,
                    c.Genres,
                    c.Date,
                    Venue = c.Venue.ToString(),
                    c.Url,
                }),
            })
            .OrderBy(g => g.Date)
            .ToList();

        memoryCache.Set(CacheKey, payload, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(15),
        });

        return new OkObjectResult(payload);
    }
}
