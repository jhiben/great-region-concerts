using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public partial class ReflektorConcertsScraper(IHttpClientFactory httpClientFactory)
    : IConcertsScraper
{
    private const string _baseUrl = "https://reflektor.be/en/agenda/";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient("scraper");
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var concertElements = document.QuerySelectorAll("ul.agenda_list li.item");

        foreach (var element in concertElements)
        {
            var bandName = element.QuerySelector("article h2 strong")?.TextContent.Trim();

            var eventUrl = element.QuerySelector("a")?.GetAttribute("href");
            if (eventUrl is not null && !eventUrl.StartsWith("http"))
                eventUrl = "https://reflektor.be" + eventUrl;

            var dateText = element
                .QuerySelector("aside.infos span:first-child")
                ?.TextContent.Trim();

            if (string.IsNullOrEmpty(bandName) || string.IsNullOrEmpty(dateText))
                continue;

            // Strip ordinal suffix (st, nd, rd, th) from the day number
            var normalizedDate = OrdinalSuffixRegex().Replace(dateText, "$1");

            if (
                !DateTime.TryParseExact(
                    normalizedDate,
                    "d MMMM yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate
                )
            )
                continue;

            var dateTime = parsedDate;

            // Extract door time from "Doors — 16:00" (time part may be absent)
            var doorText = element.QuerySelector("h5.porte")?.TextContent.Trim();
            if (!string.IsNullOrEmpty(doorText))
            {
                var timeMatch = TimeRegex().Match(doorText);
                if (timeMatch.Success)
                {
                    var hours = int.Parse(timeMatch.Groups[1].Value);
                    var minutes = int.Parse(timeMatch.Groups[2].Value);
                    dateTime = dateTime.AddHours(hours).AddMinutes(minutes);
                }
            }

            concerts.Add(
                new Concert(bandName, new DateTimeOffset(dateTime, TimeSpan.Zero), Venues.ReflektorLiege)
                {
                    Url = eventUrl,
                }
            );
        }

        return concerts;
    }

    [GeneratedRegex(@"(\d+)(?:st|nd|rd|th)")]
    private static partial Regex OrdinalSuffixRegex();

    [GeneratedRegex(@"(\d{1,2}):(\d{2})")]
    private static partial Regex TimeRegex();
}
