using BookShelf.Api.Contracts;
using BookShelf.Api.Data;
using BookShelf.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Api.Controllers;

[ApiController]
[Route("api/books")]
[Produces("application/json")]
public sealed class BooksController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<BookResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<BookResponse>>> GetAll([FromQuery] string? search,
        [FromQuery] ReadingStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 50);
        var query = db.Books.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(book => book.Title.ToLower().Contains(term) || book.Author.ToLower().Contains(term));
        }
        if (status.HasValue) query = query.Where(book => book.Status == status);
        var total = await query.CountAsync(cancellationToken);
        var books = await query.OrderBy(book => book.Title).Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync(cancellationToken);
        return Ok(new PagedResponse<BookResponse>(books.Select(ToResponse).ToList(), total, page, pageSize,
            Math.Max(1, (int)Math.Ceiling(total / (double)pageSize))));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await db.Books.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return book is null ? Problem(statusCode: 404, title: "Book not found") : Ok(ToResponse(book));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<LibrarySummary>> GetSummary(CancellationToken cancellationToken)
    {
        var books = await db.Books.AsNoTracking().ToListAsync(cancellationToken);
        var completed = books.Count(book => book.Status == ReadingStatus.Completed);
        return Ok(new LibrarySummary(books.Count, books.Count(book => book.Status == ReadingStatus.Reading), completed,
            books.Count(book => book.Status == ReadingStatus.WantToRead), books.Sum(book => book.PagesRead),
            books.Count == 0 ? 0 : (int)Math.Round(completed * 100d / books.Count)));
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
    {
        var book = FromRequest(request); db.Books.Add(book); await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, ToResponse(book));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookResponse>> Update(Guid id, BookRequest request, CancellationToken cancellationToken)
    {
        var book = await db.Books.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (book is null) return Problem(statusCode: 404, title: "Book not found");
        Apply(request, book); book.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(book));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var book = await db.Books.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (book is null) return Problem(statusCode: 404, title: "Book not found");
        db.Books.Remove(book); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private static BookResponse ToResponse(Book book) => new(book.Id, book.Title, book.Author, book.Isbn, book.Genre,
        book.TotalPages, book.PagesRead, book.Status, book.Rating, book.Notes, ReadingInsights.Calculate(book),
        book.CreatedAt, book.UpdatedAt);

    private static Book FromRequest(BookRequest request)
    {
        var book = new Book { Title = request.Title.Trim(), Author = request.Author.Trim(), Genre = request.Genre.Trim() };
        Apply(request, book); return book;
    }

    private static void Apply(BookRequest request, Book book)
    {
        book.Title = request.Title.Trim(); book.Author = request.Author.Trim(); book.Genre = request.Genre.Trim();
        book.Isbn = string.IsNullOrWhiteSpace(request.Isbn) ? null : request.Isbn.Trim();
        book.TotalPages = request.TotalPages; book.PagesRead = request.PagesRead; book.Status = request.Status;
        book.Rating = request.Rating; book.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
    }
}
