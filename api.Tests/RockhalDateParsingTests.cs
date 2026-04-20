using Api.Scrapers;

namespace Api.Tests;

public class RockhalDateParsingTests
{
    [Fact]
    public void TryParseDate_ValidDate_ReturnsTrue()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed 16 Apr 2025 | 20:00", out var date);

        Assert.True(result);
        Assert.NotNull(date);
        Assert.Equal(new DateTime(2025, 4, 16, 20, 0, 0), date.Value);
    }

    [Fact]
    public void TryParseDate_AllMonths_ParseCorrectly()
    {
        var result = RockhalConcertsScraper.TryParseDate("Mon 1 Jan 2025 | 18:30", out var date);

        Assert.True(result);
        Assert.Equal(new DateTime(2025, 1, 1, 18, 30, 0), date!.Value);
    }

    [Fact]
    public void TryParseDate_MissingParts_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed 16 Apr", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_InvalidMonthName_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed 16 Xyz 2025 | 20:00", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_EmptyString_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_MalformedTime_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed 16 Apr 2025 | bad", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_NonNumericDay_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed XX Apr 2025 | 20:00", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseDate_NonNumericYear_ReturnsFalse()
    {
        var result = RockhalConcertsScraper.TryParseDate("Wed 16 Apr YYYY | 20:00", out _);

        Assert.False(result);
    }
}
