using BookShelf.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(LibraryDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Books.AnyAsync(cancellationToken)) return;
        db.Books.AddRange(
            NewBook("Clean Architecture", "Robert C. Martin", "Software", 432, 318, ReadingStatus.Reading, "9780134494166"),
            NewBook("The Pragmatic Programmer", "David Thomas & Andrew Hunt", "Software", 352, 352, ReadingStatus.Completed, "9780135957059", 5),
            NewBook("Designing Data-Intensive Applications", "Martin Kleppmann", "Engineering", 616, 154, ReadingStatus.Reading, "9781449373320"),
            NewBook("Refactoring", "Martin Fowler", "Software", 448, 0, ReadingStatus.WantToRead, "9780134757599"),
            NewBook("Domain-Driven Design", "Eric Evans", "Architecture", 560, 88, ReadingStatus.Paused, "9780321125217"));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Book NewBook(string title, string author, string genre, int totalPages, int pagesRead, ReadingStatus status, string isbn, int? rating = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            Genre = genre,
            TotalPages = totalPages,
            PagesRead = pagesRead,
            Status = status,
            Isbn = isbn,
            Rating = rating,
            Notes = status == ReadingStatus.Reading ? "Weekly review in progress." : null
        };
}
