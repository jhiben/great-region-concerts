using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

/// <summary>
/// Scrapes concerts from the TUFA Trier website.
/// The site is built with SvelteKit; event data is extracted from JSON-LD structured data
/// embedded in the calendar page and individual event pages.
/// </summary>
public partial class TufaTrierConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.tufa-trier.de/de/veranstaltungskalender";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();
        var httpClient = httpClientFactory.CreateClient();

        // Fetch the calendar page which contains a JSON-LD ItemList with all event URLs
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        // Extract JSON-LD scripts — the ItemList contains links to all events
        var ldScripts = document.QuerySelectorAll("script[type='application/ld+json']");

        var eventUrls = new List<string>();
        foreach (var script in ldScripts)
        {
            var json = script.TextContent;
            if (!json.Contains("\"ItemList\""))
                continue;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("itemListElement", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("url", out var url))
                    {
                        eventUrls.Add(url.GetString()!);
                    }
                }
            }
        }

        // Fetch each event page and extract JSON-LD Event data
        var tasks = eventUrls.Select(url => FetchEventAsync(httpClient, url));
        var results = await Task.WhenAll(tasks);

        foreach (var concert in results)
        {
            if (concert is not null)
                concerts.Add(concert);
        }

        return concerts;
    }

    private static async Task<Concert?> FetchEventAsync(HttpClient httpClient, string eventUrl)
    {
        try
        {
            var html = await httpClient.GetStringAsync(eventUrl);
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            foreach (var script in document.QuerySelectorAll("script[type='application/ld+json']"))
            {
                var json = script.TextContent;
                if (!json.Contains("\"Event\""))
                    continue;

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("@type", out var type) && type.GetString() == "Event")
                {
                    var name = root.TryGetProperty("name", out var n) ? n.GetString()?.Trim() : null;
                    var dateStr = root.TryGetProperty("startDate", out var d) ? d.GetString() : null;

                    if (!string.IsNullOrEmpty(name) && DateTimeOffset.TryParse(dateStr, out var parsedDate))
                    {
                        return new Concert(name, parsedDate, Venues.TufaTrier)
                        {
                            Url = eventUrl,
                        };
                    }
                }
            }
        }
        catch
        {
            // Skip events that fail to load
        }

        return null;
    }
}
