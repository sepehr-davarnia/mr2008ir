using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Infrastructure.Data.Migrations
{
    public partial class AddSiteSettingBranding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaviconMediaId",
                table: "SiteSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogoMediaId",
                table: "SiteSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteName",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FaviconMediaId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "LogoMediaId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SiteName",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "SiteSettings");
        }
    }
}
