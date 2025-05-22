using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class KulturfabrikConcertsScraper : IConcertsScraper
{
    private const string _baseUrl = "https://kulturfabrik.lu/events";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Configure AngleSharp
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        // Load the website
        var document = await context.OpenAsync(_baseUrl);

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
                                ?.Where(g => g.ToLowerInvariant() is not "musique" or "music")
                                .ToArray() ?? [],
                    }
                );
            }
        }

        return concerts;
    }

    private static bool TryParseDate(string dateString, out DateTime dateTime)
    {
        dateTime = default;
        try
        {
            // Example: "Wed 21.05.25 — 19h00"
            var parts = dateString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return false;

            var dateParts = parts[1].Split('.');
            if (dateParts.Length != 3)
                return false;

            int day = int.Parse(dateParts[0]);
            int month = int.Parse(dateParts[1]);
            int year = 2000 + int.Parse(dateParts[2]); // "25" -> 2025

            var timeParts = parts[3].Split('h');
            int hour = int.Parse(timeParts[0]);
            int minute = int.Parse(timeParts[1]);

            dateTime = new DateTime(year, month, day, hour, minute, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
