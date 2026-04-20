using Api.Scrapers;

namespace Api.Tests;

public class KulturfabrikDateParsingTests
{
    [Fact]
    public void TryParseDate_ValidDate_ReturnsTrue()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Wed 21.05.25 — 19h00", out var date);

        Assert.True(result);
        Assert.Equal(new DateTime(2025, 5, 21, 19, 0, 0), date);
    }

    [Fact]
    public void TryParseDate_SingleDigitDayAndMonth_ReturnsTrue()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Mon 1.2.25 — 9h00", out var date);

        Assert.True(result);
        Assert.Equal(new DateTime(2025, 2, 1, 9, 0, 0), date);
    }

    [Fact]
    public void TryParseDate_TwoDigitYearMapsTo2000s()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Fri 15.12.30 — 20h30", out var date);

        Assert.True(result);
        Assert.Equal(new DateTime(2030, 12, 15, 20, 30, 0), date);
    }

    [Fact]
    public void TryParseDate_MissingTimePart_ReturnsFalse()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Wed 21.05.25", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_MalformedDate_ReturnsFalse()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Invalid", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_EmptyString_ReturnsFalse()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_NonNumericDateParts_ReturnsFalse()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Wed ab.cd.ef — 19h00", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_NonNumericTimeParts_ReturnsFalse()
    {
        var result = KulturfabrikConcertsScraper.TryParseDate("Wed 21.05.25 — xxhyy", out _);

        Assert.False(result);
    }
}
