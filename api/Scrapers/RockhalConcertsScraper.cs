using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class RockhalConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://rockhal.lu/agenda-en/";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Fetch HTML via IHttpClientFactory and parse with AngleSharp
        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Select the elements containing concert information
        var concertElements = document.QuerySelectorAll(".agenda-item"); // Adjust selector based on the website's structure

        foreach (var concertElement in concertElements)
        {
            var bandElement = concertElement.QuerySelector(".agenda-item__title h3.name");
            var bandName = bandElement?.FirstChild?.TextContent.Trim();
            var genre = bandElement?.QuerySelector("span")?.TextContent.Trim();

            var eventUrl = concertElement.QuerySelector("a")?.GetAttribute("href");
            if (eventUrl is not null && !eventUrl.StartsWith("http"))
                eventUrl = "https://rockhal.lu" + eventUrl;

            // Extract date
            var date = concertElement.QuerySelector(".agenda-item__date")?.TextContent.Trim();

            if (
                !string.IsNullOrEmpty(bandName)
                && !string.IsNullOrEmpty(date)
                && TryParseDate(date, out var parsedDate)
            )
            {
                concerts.Add(
                    new Concert(
                        bandName,
                        new DateTimeOffset(parsedDate.Value, TimeSpan.Zero),
                        Venues.Rockhal
                    )
                    {
                        Genres = genre
                            ?.Split('/', StringSplitOptions.RemoveEmptyEntries)
                            .Select(g => g.Trim())
                            .ToArray(),
                        Url = eventUrl,
                    }
                );
            }
        }

        return concerts;
    }

    internal static bool TryParseDate(string dateString, [NotNullWhen(true)] out DateTime? dateTime)
    {
        dateTime = null;

        var parts = dateString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 6)
            return false;

        if (!int.TryParse(parts[1], out int day))
            return false;

        var month =
            DateTimeFormatInfo.InvariantInfo.AbbreviatedMonthNames.ToList().IndexOf(parts[2]) + 1;
        if (month <= 0)
            return false;

        if (!int.TryParse(parts[3], out int year))
            return false;

        var timeParts = parts[5].Split(':');
        if (timeParts.Length != 2)
            return false;

        if (!int.TryParse(timeParts[0], out int hour)
            || !int.TryParse(timeParts[1], out int minute))
            return false;

        dateTime = new DateTime(year, month, day, hour, minute, 0);
        return true;
    }
}
