using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Infrastructure.Data.Migrations
{
    public partial class AddSiteSettingMaxUploadSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxUploadSizeKb",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 5120);

            migrationBuilder.Sql(
                "IF NOT EXISTS (SELECT 1 FROM SiteSettings)\n" +
                "BEGIN\n" +
                "    INSERT INTO SiteSettings ([Key], [Value], MaxUploadSizeKb, CreatedAt)\n" +
                "    VALUES (N'General', N'', 5120, SYSUTCDATETIME());\n" +
                "END");

            migrationBuilder.Sql(
                "WITH Ordered AS (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM SiteSettings)\n" +
                "DELETE FROM SiteSettings WHERE Id IN (SELECT Id FROM Ordered WHERE rn > 1);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxUploadSizeKb",
                table: "SiteSettings");
        }
    }
}
