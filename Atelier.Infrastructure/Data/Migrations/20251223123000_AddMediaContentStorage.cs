using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaContentStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Media",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageId",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MediaContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaContents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaContents");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "StorageId",
                table: "Media");
        }
    }
}
