using BookShelf.Domain;

namespace BookShelf.Tests;

public sealed class ReadingInsightsTests
{
    [Fact]
    public void Calculate_ReturnsProgressAndReadingProjection()
    {
        var book = new Book { Title = "Test", Author = "Author", Genre = "Tech", TotalPages = 300, PagesRead = 75 };
        var result = ReadingInsights.Calculate(book, pagesPerDay: 25, pagesPerHour: 30);
        Assert.Equal(25, result.ProgressPercent);
        Assert.Equal(225, result.RemainingPages);
        Assert.Equal(450, result.EstimatedMinutes);
        Assert.Equal(9, result.EstimatedDays);
    }

    [Theory]
    [InlineData("9780134494166", true)]
    [InlineData("0-306-40615-2", true)]
    [InlineData("9780134494167", false)]
    [InlineData("123", false)]
    [InlineData(null, true)]
    public void IsValidIsbn_ValidatesCheckDigit(string? isbn, bool expected) =>
        Assert.Equal(expected, ReadingInsights.IsValidIsbn(isbn));
}
