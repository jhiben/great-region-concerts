using System.Text.Json;
using Api.Models;

namespace Api.Scrapers;

/// <summary>
/// Scrapes concerts from Les Trinitaires (Metz) using the WordPress Tribe Events REST API.
/// Endpoint: /wp-json/tribe/events/v1/events
/// </summary>
public class TrinitairesConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl =
        "https://www.maisontrinitaires.com/wp-json/tribe/events/v1/events";
    private const int _pageSize = 50;

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();
        var httpClient = httpClientFactory.CreateClient("scraper");

        var page = 1;
        int totalPages;

        do
        {
            var url = $"{_baseUrl}?per_page={_pageSize}&page={page}";
            var json = await httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            totalPages = root.TryGetProperty("total_pages", out var tp) ? tp.GetInt32() : 1;

            if (root.TryGetProperty("events", out var events))
            {
                foreach (var evt in events.EnumerateArray())
                {
                    var concert = ParseEvent(evt);
                    if (concert is not null)
                        concerts.Add(concert);
                }
            }

            page++;
        } while (page <= totalPages);

        return concerts;
    }

    private static Concert? ParseEvent(JsonElement evt)
    {
        var title = evt.TryGetProperty("title", out var t) ? t.GetString()?.Trim() : null;
        var startDate = evt.TryGetProperty("start_date", out var sd) ? sd.GetString() : null;
        var eventUrl = evt.TryGetProperty("url", out var u) ? u.GetString() : null;

        // Extract categories as genres
        string[]? genres = null;
        if (evt.TryGetProperty("categories", out var cats))
        {
            var categoryNames = new List<string>();
            foreach (var cat in cats.EnumerateArray())
            {
                if (cat.TryGetProperty("name", out var name))
                {
                    var catName = name.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(catName))
                        categoryNames.Add(catName);
                }
            }

            if (categoryNames.Count > 0)
                genres = [.. categoryNames];
        }

        if (string.IsNullOrEmpty(title) || !DateTimeOffset.TryParse(startDate, out var parsedDate))
            return null;

        // Decode HTML entities in the title (e.g., &#038; -> &)
        title = System.Net.WebUtility.HtmlDecode(title);

        return new Concert(title, parsedDate, Venues.Trinitaires)
        {
            Genres = genres,
            Url = eventUrl,
        };
    }
}
