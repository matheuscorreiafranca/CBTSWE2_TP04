using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookShelf.Api.Contracts;
using BookShelf.Api.Data;
using BookShelf.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookShelf.Tests;

public sealed class BooksApiTests : IClassFixture<BookShelfFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client;
    public BooksApiTests(BookShelfFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task CrudFlow_CreatesUpdatesAndDeletesBook()
    {
        var request = new BookRequest
        {
            Title = "Patterns of Enterprise Application Architecture",
            Author = "Martin Fowler",
            Genre = "Software",
            Isbn = "9780321127426",
            TotalPages = 560,
            PagesRead = 0,
            Status = ReadingStatus.WantToRead
        };
        var create = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<BookResponse>(JsonOptions);
        Assert.NotNull(created);

        var update = request.WithChanges(pagesRead: 100, status: ReadingStatus.Reading);
        var updatedResponse = await _client.PutAsJsonAsync($"/api/books/{created.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updatedResponse.StatusCode);
        var updated = await updatedResponse.Content.ReadFromJsonAsync<BookResponse>(JsonOptions);
        Assert.Equal(18, updated?.Projection.ProgressPercent);

        var delete = await _client.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/books/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Create_WithPagesBeyondTotal_ReturnsValidationProblem()
    {
        var invalid = new BookRequest
        {
            Title = "Invalid Book",
            Author = "Test Author",
            Genre = "Test",
            TotalPages = 100,
            PagesRead = 101,
            Status = ReadingStatus.Reading
        };
        var response = await _client.PostAsJsonAsync("/api/books", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

internal static class BookRequestTestExtensions
{
    public static BookRequest WithChanges(this BookRequest request, int pagesRead, ReadingStatus status) => new()
    {
        Title = request.Title,
        Author = request.Author,
        Genre = request.Genre,
        Isbn = request.Isbn,
        TotalPages = request.TotalPages,
        PagesRead = pagesRead,
        Status = status,
        Rating = request.Rating,
        Notes = request.Notes
    };
}

public sealed class BookShelfFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LibraryDbContext>>();
            services.AddDbContext<LibraryDbContext>(options => options.UseSqlite(_connection));
        });
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) _connection.Dispose(); }
}
