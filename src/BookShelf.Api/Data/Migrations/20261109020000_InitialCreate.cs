using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShelf.Api.Data.Migrations;

[DbContext(typeof(LibraryDbContext))]
[Migration("20261109020000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Books",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                Author = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Isbn = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                Genre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                TotalPages = table.Column<int>(type: "INTEGER", nullable: false),
                PagesRead = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                Rating = table.Column<int>(type: "INTEGER", nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Books", item => item.Id));

        migrationBuilder.CreateIndex(name: "IX_Books_Status", table: "Books", column: "Status");
        migrationBuilder.CreateIndex(name: "IX_Books_Title", table: "Books", column: "Title");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Books");
}
