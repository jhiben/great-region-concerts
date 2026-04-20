using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class PhilharmonieConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.philharmonie.lu/en/programme";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient("scraper");
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var concertElements = document.QuerySelectorAll(".c-event-list-item");

        foreach (var concertElement in concertElements)
        {
            var bandName = concertElement
                .QuerySelector(".event-list-item__content h5")
                ?.TextContent.Trim();

            var dateString = concertElement
                .QuerySelector("time.event-list-item__date-date")
                ?.GetAttribute("datetime");

            var timeString = concertElement
                .QuerySelector("span.event-list-item__date-time")
                ?.GetAttribute("datetime");

            var date = ParseDate(dateString, timeString);

            var genres = concertElement
                .QuerySelectorAll(".tag-list__tag .button__label")
                .Select(e => e.TextContent.Trim())
                .Where(g => !string.IsNullOrEmpty(g))
                .ToArray();

            var eventUrl = concertElement.QuerySelector("a")?.GetAttribute("href");
            if (eventUrl is not null && !eventUrl.StartsWith("http"))
                eventUrl = "https://www.philharmonie.lu" + eventUrl;

            if (!string.IsNullOrEmpty(bandName) && date is not null)
            {
                concerts.Add(
                    new Concert(bandName, date.Value, Venues.Philharmonie)
                    {
                        Genres = genres.Length > 0 ? genres : null,
                        Url = eventUrl,
                    }
                );
            }
        }

        return concerts;
    }

    internal static DateTimeOffset? ParseDate(string? dateString, string? timeString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        if (!DateTime.TryParse(dateString, out var date))
            return null;

        if (
            !string.IsNullOrEmpty(timeString)
            && TimeSpan.TryParse(timeString, out var time)
        )
        {
            date = date.Date + time;
        }

        return new DateTimeOffset(date, TimeSpan.Zero);
    }
}
