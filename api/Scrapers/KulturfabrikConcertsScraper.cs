using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class KulturfabrikConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://kulturfabrik.lu/events";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Fetch HTML via IHttpClientFactory and parse with AngleSharp
        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Select the elements containing concert information
        var concertElements = document.QuerySelectorAll(".list-item");

        foreach (var concertElement in concertElements)
        {
            var genres = concertElement
                .QuerySelector(".item-category")
                ?.TextContent?.Trim()
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToArray();

            if (
                genres is not null
                && !genres.Contains("music", StringComparer.OrdinalIgnoreCase)
                && !genres.Contains("musique", StringComparer.OrdinalIgnoreCase)
            )
            {
                continue; // Skip if the genre is not music
            }

            var bandName = concertElement.QuerySelector(".item-title")?.TextContent.Trim();

            // Extract date
            var dateString = concertElement.QuerySelector(".item-date")?.TextContent.Trim();

            if (
                !string.IsNullOrEmpty(bandName)
                && dateString is not null
                && TryParseDate(dateString, out var date)
            )
            {
                concerts.Add(
                    new Concert(bandName, new(date, TimeSpan.Zero), Venue.Kulturfabrik)
                    {
                        Genres =
                            genres
                                ?.Where(g => g.ToLowerInvariant() is not "musique" and not "music")
                                .ToArray() ?? [],
                    }
                );
            }
        }

        return concerts;
    }

    internal static bool TryParseDate(string dateString, out DateTime dateTime)
    {
        dateTime = default;

        // Example: "Wed 21.05.25 — 19h00"
        var parts = dateString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4)
            return false;

        var dateParts = parts[1].Split('.');
        if (dateParts.Length != 3)
            return false;

        if (!int.TryParse(dateParts[0], out int day)
            || !int.TryParse(dateParts[1], out int month)
            || !int.TryParse(dateParts[2], out int yearShort))
            return false;

        int year = 2000 + yearShort; // "25" -> 2025

        var timeParts = parts[3].Split('h');
        if (timeParts.Length != 2)
            return false;

        if (!int.TryParse(timeParts[0], out int hour)
            || !int.TryParse(timeParts[1], out int minute))
            return false;

        dateTime = new DateTime(year, month, day, hour, minute, 0);
        return true;
    }
}
