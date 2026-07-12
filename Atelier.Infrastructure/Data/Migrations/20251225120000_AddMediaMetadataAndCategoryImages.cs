using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Infrastructure.Data.Migrations
{
    public partial class AddMediaMetadataAndCategoryImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Media",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotDownloaded",
                table: "Media",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Media",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Media",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultCategoryMediaId",
                table: "SiteSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeHeroMediaId",
                table: "SiteSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeSecondaryMediaId",
                table: "SiteSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MediaId",
                table: "Categories",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Media_MediaId",
                table: "Categories",
                column: "MediaId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Media_MediaId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_MediaId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "IsNotDownloaded",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "DefaultCategoryMediaId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HomeHeroMediaId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HomeSecondaryMediaId",
                table: "SiteSettings");
        }
    }
}
