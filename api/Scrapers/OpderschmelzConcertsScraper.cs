using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class OpderschmelzConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://opderschmelz.lu";

    private static readonly HashSet<string> _musicGenres = new(StringComparer.OrdinalIgnoreCase)
    {
        "Concert",
        "Jazz",
        "Rock",
        "Blues",
        "Celtic Music & Dance",
        "Festival",
    };

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var linkElements = document.QuerySelectorAll("a[href*=\"agenda\"]");

        foreach (var link in linkElements)
        {
            var galleryText = link.QuerySelector(".event-detail__gallery-text");
            if (galleryText is null)
                continue;

            var genre = link.QuerySelector(".event-detail__new-category")?.TextContent.Trim();
            if (string.IsNullOrEmpty(genre) || !_musicGenres.Contains(genre))
                continue;

            var bandName = galleryText.QuerySelector("h3")?.TextContent.Trim();
            var dateString = galleryText.QuerySelector("h2")?.TextContent.Trim();

            if (
                !string.IsNullOrEmpty(bandName)
                && dateString is not null
                && TryParseDate(dateString, out var date)
            )
            {
                var href = link.GetAttribute("href");
                var url = href is not null ? _baseUrl + href : null;

                concerts.Add(
                    new Concert(bandName, new(date, TimeSpan.Zero), Venues.Opderschmelz)
                    {
                        Genres = [genre],
                        Url = url,
                    }
                );
            }
        }

        return concerts;
    }

    internal static bool TryParseDate(string dateString, out DateTime dateTime)
    {
        dateTime = default;

        // Format: "DD ― MM ― YYYY" optionally followed by "HHhMM"
        var parts = dateString.Split('―', StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int day) || !int.TryParse(parts[1], out int month))
            return false;

        // The year part may contain a time suffix like "2026 20h00"
        var yearAndTime = parts[2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (yearAndTime.Length < 1 || !int.TryParse(yearAndTime[0], out int year))
            return false;

        int hour = 0, minute = 0;
        if (yearAndTime.Length > 1)
        {
            var timeParts = yearAndTime[1].Split('h');
            if (
                timeParts.Length == 2
                && int.TryParse(timeParts[0], out int h)
                && int.TryParse(timeParts[1], out int m)
            )
            {
                hour = h;
                minute = m;
            }
        }

        dateTime = new DateTime(year, month, day, hour, minute, 0);
        return true;
    }
}
