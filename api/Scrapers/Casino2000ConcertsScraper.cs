using System.Globalization;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class Casino2000ConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://casino2000.lu/en/events/";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Events are in div.wrapper elements containing an h1 (title) and a td with the date
        var wrapperElements = document.QuerySelectorAll("div.wrapper");

        foreach (var wrapper in wrapperElements)
        {
            var bandName = wrapper.QuerySelector("h1")?.TextContent.Trim();

            var eventUrl = wrapper.QuerySelector("a")?.GetAttribute("href");
            if (eventUrl is not null && !eventUrl.StartsWith("http"))
                eventUrl = "https://casino2000.lu" + eventUrl;

            var dateCell = wrapper.QuerySelector("td");
            var dateText = dateCell?.TextContent.Trim()
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?.Trim();

            if (
                !string.IsNullOrEmpty(bandName)
                && !string.IsNullOrEmpty(dateText)
                && DateTime.TryParseExact(
                    dateText,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate
                )
            )
            {
                concerts.Add(
                    new Concert(
                        bandName,
                        new DateTimeOffset(parsedDate, TimeSpan.Zero),
                        Venues.Casino2000
                    )
                    {
                        Url = eventUrl,
                    }
                );
            }
        }

        return concerts;
    }
}
