using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStorage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BookFile_FileType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "BookFiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "BookFiles");
        }
    }
}
