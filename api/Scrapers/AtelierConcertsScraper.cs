using AngleSharp;
using Api.Models;

namespace Api.Scrapers;


public class AtelierConcertsScraper : IConcertsScraper
{
    private const string _baseUrl = "https://www.atelier.lu/#agenda"; // Adjust the URL as needed

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();

        // Configure AngleSharp
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        // Load the website
        var document = await context.OpenAsync(_baseUrl);

        // Select the elements containing concert information
        var concertElements = document.QuerySelectorAll("[itemtype='http://schema.org/Event']"); // Adjust selector based on the website's structure

        foreach (var concertElement in concertElements)
        {
            var bandName = concertElement
                .QuerySelector("meta[itemprop='name']")
                ?.GetAttribute("content")
                ?.Trim();

            // Extract date
            var dateString = concertElement
                .QuerySelector("meta[itemprop='startDate']")
                ?.GetAttribute("content");
            var date = DateTimeOffset.TryParse(dateString, out var parsedDate)
                ? (DateTimeOffset?)parsedDate
                : null;

            if (!string.IsNullOrEmpty(bandName) && date is not null)
            {
                concerts.Add(new Concert(bandName, date.Value, Venue.Atelier));
            }
        }

        return concerts;
    }
}
