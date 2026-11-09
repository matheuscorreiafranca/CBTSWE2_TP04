using BookShelf.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Api.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<Book>();
        book.ToTable("Books");
        book.HasKey(item => item.Id);
        book.Property(item => item.Title).HasMaxLength(160).IsRequired();
        book.Property(item => item.Author).HasMaxLength(120).IsRequired();
        book.Property(item => item.Genre).HasMaxLength(80).IsRequired();
        book.Property(item => item.Isbn).HasMaxLength(17);
        book.Property(item => item.Notes).HasMaxLength(1000);
        book.Property(item => item.Status).HasConversion<string>().HasMaxLength(24);
        book.HasIndex(item => item.Title);
        book.HasIndex(item => item.Status);
    }
}
