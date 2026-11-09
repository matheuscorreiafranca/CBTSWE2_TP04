using System.ComponentModel.DataAnnotations;
using BookShelf.Domain;

namespace BookShelf.Api.Contracts;

public sealed record BookResponse(Guid Id, string Title, string Author, string? Isbn, string Genre, int TotalPages,
    int PagesRead, ReadingStatus Status, int? Rating, string? Notes, ReadingProjection Projection,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed class BookRequest : IValidatableObject
{
    [Required, StringLength(160, MinimumLength = 2)] public string Title { get; init; } = string.Empty;
    [Required, StringLength(120, MinimumLength = 2)] public string Author { get; init; } = string.Empty;
    [StringLength(17)] public string? Isbn { get; init; }
    [Required, StringLength(80, MinimumLength = 2)] public string Genre { get; init; } = string.Empty;
    [Range(1, 20_000)] public int TotalPages { get; init; }
    [Range(0, 20_000)] public int PagesRead { get; init; }
    [EnumDataType(typeof(ReadingStatus))] public ReadingStatus Status { get; init; }
    [Range(1, 5)] public int? Rating { get; init; }
    [StringLength(1000)] public string? Notes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PagesRead > TotalPages)
            yield return new ValidationResult("Pages read cannot exceed total pages.", [nameof(PagesRead)]);
        if (!ReadingInsights.IsValidIsbn(Isbn))
            yield return new ValidationResult("ISBN check digit is invalid.", [nameof(Isbn)]);
        if (Status == ReadingStatus.Completed && PagesRead != TotalPages)
            yield return new ValidationResult("A completed book must have all pages read.", [nameof(PagesRead), nameof(Status)]);
    }
}

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize, int TotalPages);
public sealed record LibrarySummary(int Total, int Reading, int Completed, int WantToRead, int PagesRead, int CompletionRate);
