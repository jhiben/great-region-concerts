using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class AtelierConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.atelier.lu/#agenda";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Fetch HTML via IHttpClientFactory and parse with AngleSharp
        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Select the elements containing concert information
        var concertElements = document.QuerySelectorAll("[itemtype='http://schema.org/Event']"); // Adjust selector based on the website's structure

        foreach (var concertElement in concertElements)
        {
            var bandName = concertElement
                .QuerySelector("meta[itemprop='name']")
                ?.GetAttribute("content")
                ?.Trim();

            // Extract date
            var dateString = concertElement
                .QuerySelector("meta[itemprop='startDate']")
                ?.GetAttribute("content");
            var date = DateTimeOffset.TryParse(dateString, out var parsedDate)
                ? (DateTimeOffset?)parsedDate
                : null;

            if (!string.IsNullOrEmpty(bandName) && date is not null)
            {
                concerts.Add(new Concert(bandName, date.Value, Venues.Atelier));
            }
        }

        return concerts;
    }
}
