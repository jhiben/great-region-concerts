using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

/// <summary>
/// Scrapes concerts from Cité musicale-Metz (Arsenal, Trinitaires, BAM).
/// Source: https://www.citemusicale-metz.fr/programmation
/// </summary>
public partial class CiteMusicaleSconcertsScraper(IHttpClientFactory httpClientFactory)
    : IConcertsScraper
{
    private const string _baseUrl = "https://www.citemusicale-metz.fr/programmation";
    private const int _maxPages = 3;

    private static readonly Dictionary<string, Venue> _venueMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Arsenal"] = Venues.Arsenal,
        ["Trinitaires"] = Venues.Trinitaires,
        ["BAM"] = Venues.BAM,
    };

    private static readonly Dictionary<string, int> _frenchMonths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["janv."] = 1, ["janvier"] = 1,
        ["fév."] = 2, ["février"] = 2,
        ["mars"] = 3,
        ["avr."] = 4, ["avril"] = 4,
        ["mai"] = 5,
        ["juin"] = 6,
        ["juil."] = 7, ["juillet"] = 7,
        ["août"] = 8,
        ["sept."] = 9, ["septembre"] = 9,
        ["oct."] = 10, ["octobre"] = 10,
        ["nov."] = 11, ["novembre"] = 11,
        ["déc."] = 12, ["décembre"] = 12,
    };

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();
        var httpClient = httpClientFactory.CreateClient();
        var browsingContext = BrowsingContext.New(Configuration.Default);

        for (var page = 1; page <= _maxPages; page++)
        {
            var url = page == 1 ? _baseUrl : $"{_baseUrl}?page={page}";

            string html;
            try
            {
                html = await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                break;
            }

            var document = await browsingContext.OpenAsync(req => req.Content(html));
            var cards = document.QuerySelectorAll("div.card_A2cIc");

            if (cards.Length == 0)
                break;

            foreach (var card in cards)
            {
                var concert = ParseCard(card);
                if (concert is not null)
                    concerts.Add(concert);
            }
        }

        return concerts;
    }

    private static Concert? ParseCard(AngleSharp.Dom.IElement card)
    {
        // Event type (Concert, Exposition, Spectacle, etc.)
        var typeText = card.QuerySelector("p.over-title_xujnb")?.TextContent?.Trim();
        if (string.IsNullOrEmpty(typeText) || !typeText.Contains("Concert", StringComparison.OrdinalIgnoreCase))
            return null;

        // Title (band name)
        var title = card.QuerySelector("h3 a")?.TextContent?.Trim();
        if (string.IsNullOrEmpty(title))
            return null;

        // Venue from place element
        var placeText = card.QuerySelector("p.place_e0lY8")?.TextContent?.Trim();
        if (string.IsNullOrEmpty(placeText) || !_venueMap.TryGetValue(placeText, out var venue))
            return null;

        // Date: prefer the datetime attribute on <time> elements
        var timeElement = card.QuerySelector("div.body_IBYpL time[datetime]");
        var dateTimeAttr = timeElement?.GetAttribute("datetime");
        var date = ParseDate(dateTimeAttr, timeElement?.TextContent?.Trim());
        if (date is null)
            return null;

        // Subtitle (artist description / guests)
        var subtitle = card.QuerySelector("div.subtitle_ZIAux")?.TextContent?.Trim();

        // Event URL
        var eventUrl = card.QuerySelector("h3 a")?.GetAttribute("href");
        if (!string.IsNullOrEmpty(eventUrl) && !eventUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            eventUrl = $"https://www.citemusicale-metz.fr{eventUrl}";

        return new Concert(title, date.Value, venue)
        {
            Description = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle,
            Url = eventUrl,
        };
    }

    private static DateTimeOffset? ParseDate(string? dateTimeAttr, string? displayText)
    {
        // Try parsing the ISO datetime attribute first (e.g., "2026-04-25T18:30" or "2026-04-29")
        if (!string.IsNullOrEmpty(dateTimeAttr))
        {
            if (DateTimeOffset.TryParse(dateTimeAttr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                return parsed;
        }

        // Fallback: parse French display text like "25 avr. 2026, 20h30" or "29 avr. 2026"
        if (string.IsNullOrEmpty(displayText))
            return null;

        return ParseFrenchDate(displayText);
    }

    private static DateTimeOffset? ParseFrenchDate(string text)
    {
        // Match patterns like "7 mai 2026, 20h" or "29 avr. 2026, 20h30" or "29 avr. 2026"
        var match = FrenchDateRegex().Match(text);
        if (!match.Success)
            return null;

        var day = int.Parse(match.Groups["day"].Value);
        var monthStr = match.Groups["month"].Value;
        var year = int.Parse(match.Groups["year"].Value);

        if (!_frenchMonths.TryGetValue(monthStr, out var month))
            return null;

        var hour = match.Groups["hour"].Success ? int.Parse(match.Groups["hour"].Value) : 0;
        var minute = match.Groups["minute"].Success ? int.Parse(match.Groups["minute"].Value) : 0;

        try
        {
            var dt = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
            // Metz is in CET (UTC+1) / CEST (UTC+2)
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            var offset = tz.GetUtcOffset(dt);
            return new DateTimeOffset(dt, offset);
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"(?<day>\d{1,2})\s+(?<month>[a-zéû.]+)\s+(?<year>\d{4})(?:,\s*(?<hour>\d{1,2})h(?<minute>\d{2})?)?", RegexOptions.IgnoreCase)]
    private static partial Regex FrenchDateRegex();
}
