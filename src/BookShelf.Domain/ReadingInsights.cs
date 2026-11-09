namespace BookShelf.Domain;

public sealed record ReadingProjection(
    int ProgressPercent,
    int RemainingPages,
    int EstimatedMinutes,
    int EstimatedDays);

/// <summary>
/// Business rules compiled into BookShelf.Domain.dll and consumed by the API.
/// </summary>
public static class ReadingInsights
{
    public static ReadingProjection Calculate(Book book, int pagesPerDay = 25, int pagesPerHour = 30)
    {
        ArgumentNullException.ThrowIfNull(book);
        if (book.TotalPages <= 0) throw new ArgumentOutOfRangeException(nameof(book.TotalPages));
        if (pagesPerDay <= 0) throw new ArgumentOutOfRangeException(nameof(pagesPerDay));
        if (pagesPerHour <= 0) throw new ArgumentOutOfRangeException(nameof(pagesPerHour));

        var pagesRead = Math.Clamp(book.PagesRead, 0, book.TotalPages);
        var remaining = book.TotalPages - pagesRead;

        return new ReadingProjection(
            (int)Math.Round(pagesRead * 100d / book.TotalPages),
            remaining,
            (int)Math.Ceiling(remaining * 60d / pagesPerHour),
            (int)Math.Ceiling(remaining / (double)pagesPerDay));
    }

    public static bool IsValidIsbn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        var isbn = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return isbn.Length switch
        {
            10 => IsValidIsbn10(isbn),
            13 => IsValidIsbn13(isbn),
            _ => false
        };
    }

    private static bool IsValidIsbn10(string isbn)
    {
        var sum = 0;
        for (var index = 0; index < 10; index++)
        {
            var digit = index == 9 && isbn[index] == 'X' ? 10 : isbn[index] - '0';
            if (digit is < 0 or > 10) return false;
            sum += digit * (10 - index);
        }
        return sum % 11 == 0;
    }

    private static bool IsValidIsbn13(string isbn)
    {
        if (isbn.Any(character => !char.IsDigit(character))) return false;
        var sum = isbn.Take(12).Select((character, index) => (character - '0') * (index % 2 == 0 ? 1 : 3)).Sum();
        return (10 - sum % 10) % 10 == isbn[12] - '0';
    }
}
