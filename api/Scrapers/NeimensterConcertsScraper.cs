using System.Text.Json;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

public class NeimensterConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.neimenster.lu/en/events";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // The listing page only provides event titles and links (no dates).
        // We collect event URLs and then fetch each detail page for JSON-LD structured data.
        var articles = document.QuerySelectorAll("article.type-events");

        foreach (var article in articles)
        {
            var linkElement = article.QuerySelector("h2.entry-title a");
            var eventName = linkElement?.TextContent.Trim();
            var eventUrl = linkElement?.GetAttribute("href");

            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(eventUrl))
                continue;

            try
            {
                var eventHtml = await httpClient.GetStringAsync(eventUrl);
                var eventDocument = await context.OpenAsync(req => req.Content(eventHtml));

                var ldJsonScripts = eventDocument.QuerySelectorAll(
                    "script[type='application/ld+json']"
                );

                foreach (var script in ldJsonScripts)
                {
                    var concert = TryParseEventFromJsonLd(script.TextContent, eventName, eventUrl);
                    if (concert is not null)
                    {
                        concerts.Add(concert);
                        break;
                    }
                }
            }
            catch
            {
                // Skip events whose detail pages can't be fetched
            }
        }

        return concerts;
    }

    internal static Concert? TryParseEventFromJsonLd(string json, string fallbackName, string? eventUrl)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (
                root.TryGetProperty("@type", out var typeElement)
                && typeElement.GetString() == "Event"
            )
            {
                var name = root.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString()?.Trim()
                    : fallbackName;

                var startDateStr = root.TryGetProperty("startDate", out var dateElement)
                    ? dateElement.GetString()
                    : null;

                if (
                    !string.IsNullOrEmpty(name)
                    && !string.IsNullOrEmpty(startDateStr)
                    && DateTimeOffset.TryParse(startDateStr, out var date)
                )
                {
                    return new Concert(name, date, Venues.Neimenster) { Url = eventUrl };
                }
            }
        }
        catch
        {
            // Invalid JSON-LD, skip
        }

        return null;
    }
}
