namespace BookShelf.Domain;

public enum ReadingStatus
{
    WantToRead,
    Reading,
    Completed,
    Paused
}

public sealed class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Author { get; set; }
    public string? Isbn { get; set; }
    public required string Genre { get; set; }
    public int TotalPages { get; set; }
    public int PagesRead { get; set; }
    public ReadingStatus Status { get; set; }
    public int? Rating { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
