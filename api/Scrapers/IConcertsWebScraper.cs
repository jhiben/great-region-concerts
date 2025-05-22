using Api.Models;

namespace Api.Scrapers;

public interface IConcertsScraper
{
    Task<IEnumerable<Concert>> FetchConcerts();
}
