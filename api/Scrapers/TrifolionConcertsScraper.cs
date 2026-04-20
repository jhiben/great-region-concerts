using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public partial class TrifolionConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.trifolion.lu/en/agenda";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Each event is an <a> with class "group" containing a div.container-full-padded
        var eventLinks = document.QuerySelectorAll("a.group");

        foreach (var eventLink in eventLinks)
        {
            var bandName = eventLink.QuerySelector("h3")?.TextContent.Trim();

            // Date is in a div with "uppercase" class containing a pattern like "Mittwoch 22.04.2026"
            var dateText = ExtractDateText(eventLink.TextContent);

            // Time is in a separate element, format like "17h00"
            var timeText = ExtractTimeText(eventLink.TextContent);

            var date = ParseDate(dateText, timeText);

            if (!string.IsNullOrEmpty(bandName) && date is not null)
            {
                concerts.Add(new Concert(bandName, date.Value, Venues.Trifolion));
            }
        }

        return concerts;
    }

    internal static string? ExtractDateText(string text)
    {
        // Match date pattern dd.MM.yyyy
        var match = DatePattern().Match(text);
        return match.Success ? match.Value : null;
    }

    internal static string? ExtractTimeText(string text)
    {
        // Match time pattern like "17h00"
        var match = TimePattern().Match(text);
        return match.Success ? match.Value : null;
    }

    internal static DateTimeOffset? ParseDate(string? dateText, string? timeText)
    {
        if (string.IsNullOrEmpty(dateText))
            return null;

        if (
            !DateTime.TryParseExact(
                dateText,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            return null;
        }

        if (!string.IsNullOrEmpty(timeText))
        {
            var timeParts = timeText.Split('h');
            if (
                timeParts.Length == 2
                && int.TryParse(timeParts[0], out var hour)
                && int.TryParse(timeParts[1], out var minute)
            )
            {
                date = date.Date.AddHours(hour).AddMinutes(minute);
            }
        }

        return new DateTimeOffset(date, TimeSpan.Zero);
    }

    [GeneratedRegex(@"\d{2}\.\d{2}\.\d{4}")]
    private static partial Regex DatePattern();

    [GeneratedRegex(@"\d{1,2}h\d{2}")]
    private static partial Regex TimePattern();
}
