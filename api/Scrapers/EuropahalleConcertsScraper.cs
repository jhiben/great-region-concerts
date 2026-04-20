using System.Globalization;
using AngleSharp;
using Api.Models;

namespace Api.Scrapers;

/// <summary>
/// Scrapes concerts from the Europahalle Trier website.
/// Events are listed on the homepage as .eventvorschau_card elements with
/// date parts in .tag_num, .monat_num, .jahr and title in .titel_wrapper h2.
/// </summary>
public class EuropahalleConcertsScraper(IHttpClientFactory httpClientFactory) : IConcertsScraper
{
    private const string _baseUrl = "https://www.europahalle-trier.de";

    public async Task<IEnumerable<Concert>> FetchConcerts()
    {
        var concerts = new List<Concert>();
        var httpClient = httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(_baseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var eventCards = document.QuerySelectorAll(".eventvorschau_card");

        foreach (var card in eventCards)
        {
            var titleElement = card.QuerySelector(".titel_wrapper h2.force-nomargin");
            var bandName = titleElement?.TextContent.Trim();

            var subtitle = card.QuerySelector(".titel_wrapper h2.subtitle")?.TextContent.Trim();

            // Date parts: day in .tag_num, month in .monat_num, year in .jahr
            var dayText = card.QuerySelector(".tag_num")?.TextContent.Trim().TrimEnd('.');
            var monthText = card.QuerySelector(".monat_num")?.TextContent.Trim().TrimEnd('.');
            var yearText = card.QuerySelector(".jahr")?.TextContent.Trim();

            // Event detail link
            var linkElement = card.QuerySelector("a[href*='eventdetail']");
            var eventUrl = linkElement?.GetAttribute("href");

            if (
                !string.IsNullOrEmpty(bandName)
                && int.TryParse(dayText, out var day)
                && int.TryParse(monthText, out var month)
                && int.TryParse(yearText, out var year)
            )
            {
                var date = new DateTimeOffset(year, month, day, 20, 0, 0, TimeSpan.FromHours(1));

                concerts.Add(
                    new Concert(bandName, date, Venues.EuropahalleTrier)
                    {
                        Description = subtitle,
                        Url = eventUrl,
                    }
                );
            }
        }

        return concerts;
    }
}
