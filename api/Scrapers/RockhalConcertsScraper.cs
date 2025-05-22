using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class RockhalConcertsScraper : IConcertsScraper
{
    private const string _baseUrl = "https://rockhal.lu/agenda-en/"; // Adjust the URL as needed

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Configure AngleSharp
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        // Load the website
        var document = await context.OpenAsync(_baseUrl);

        // Select the elements containing concert information
        var concertElements = document.QuerySelectorAll(".agenda-item"); // Adjust selector based on the website's structure

        foreach (var concertElement in concertElements)
        {
            var bandElement = concertElement.QuerySelector(".agenda-item__title h3.name");
            var bandName = bandElement?.FirstChild?.TextContent.Trim(); // Adjust selector
            var genre = bandElement?.QuerySelector("span")?.TextContent.Trim(); // Adjust selector

            // Extract date
            var date = concertElement.QuerySelector(".agenda-item__date")?.TextContent.Trim();

            if (
                !string.IsNullOrEmpty(bandName)
                && !string.IsNullOrEmpty(date)
                && TryParseDate(date, out var parsedDate)
            )
            {
                // Parse the date and create a Concert object
                concerts.Add(
                    new Concert(
                        bandName,
                        new DateTimeOffset(parsedDate.Value, TimeSpan.Zero),
                        Venue.Rockhal
                    )
                    {
                        Genres = genre
                            ?.Split('/', StringSplitOptions.RemoveEmptyEntries)
                            .Select(g => g.Trim())
                            .ToArray(),
                    }
                );
            }
        }

        return concerts;
    }

    private static bool TryParseDate(string dateString, [NotNullWhen(true)] out DateTime? dateTime)
    {
        dateTime = null;

        try
        {
            var parts = dateString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var dayOfWeek = parts[0]; // e.g., "Wed"
            var day = int.Parse(parts[1]); // e.g., "16"
            var month =
                DateTimeFormatInfo.InvariantInfo.AbbreviatedMonthNames.ToList().IndexOf(parts[2])
                + 1; // Convert "Apr" to 4
            var year = int.Parse(parts[3]); // e.g., "2025"
            var timeParts = parts[5].Split(':'); // e.g., "20:00"
            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);

            dateTime = new DateTime(year, month, day, hour, minute, 0);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
