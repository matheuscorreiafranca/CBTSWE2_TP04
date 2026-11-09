using BookShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace BookShelf.Api.Data.Migrations;

[DbContext(typeof(LibraryDbContext))]
partial class LibraryDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.22");
        modelBuilder.Entity("BookShelf.Domain.Book", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
            entity.Property<string>("Author").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("TEXT");
            entity.Property<string>("Genre").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            entity.Property<string>("Isbn").HasMaxLength(17).HasColumnType("TEXT");
            entity.Property<string>("Notes").HasMaxLength(1000).HasColumnType("TEXT");
            entity.Property<int>("PagesRead").HasColumnType("INTEGER");
            entity.Property<int?>("Rating").HasColumnType("INTEGER");
            entity.Property<string>("Status").IsRequired().HasMaxLength(24).HasColumnType("TEXT");
            entity.Property<string>("Title").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            entity.Property<int>("TotalPages").HasColumnType("INTEGER");
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("Status");
            entity.HasIndex("Title");
            entity.ToTable("Books");
        });
#pragma warning restore 612, 618
    }
}
