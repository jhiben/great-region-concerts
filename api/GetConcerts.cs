using Api.Scrapers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api;

public class GetConcerts(
    ILogger<GetConcerts> logger,
    IEnumerable<IConcertsScraper> concertsScrapers
)
{
    [Function("GetConcerts")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "concerts")] HttpRequest req
    )
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var groups = (
            await Task.WhenAll(concertsScrapers.Select(scraper => scraper.FetchConcerts()))
        )
            .SelectMany(c => c)
            .GroupBy(c => c.Date);

        return new OkObjectResult(
            groups
                .Select(g => new
                {
                    Date = g.Key,
                    Concerts = g.Select(c => new
                    {
                        c.Band,
                        c.Genres,
                        c.Date,
                        Venue = c.Venue.ToString(),
                    }),
                })
                .OrderBy(g => g.Date)
        );
    }
}
