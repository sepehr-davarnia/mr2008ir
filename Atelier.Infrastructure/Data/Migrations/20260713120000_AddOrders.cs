using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Infrastructure.Data.Migrations;

[DbContext(typeof(AtelierDbContext))]
[Migration("20260713120000_AddCommerceFoundation")]
public partial class AddCommerceFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("AlternatePartNumbers", "Products", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("Brand", "Products", "nvarchar(120)", maxLength: 120, nullable: true);
        migrationBuilder.AddColumn<string>("Manufacturer", "Products", "nvarchar(160)", maxLength: 160, nullable: true);
        migrationBuilder.AddColumn<string>("OemPartNumber", "Products", "nvarchar(120)", maxLength: 120, nullable: true);
        migrationBuilder.AddColumn<string>("TechnicalPartNumber", "Products", "nvarchar(120)", maxLength: 120, nullable: true);

        migrationBuilder.CreateTable("Orders", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Number = table.Column<string>("nvarchar(32)", maxLength: 32, nullable: false),
            PublicToken = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: false),
            CustomerName = table.Column<string>("nvarchar(120)", maxLength: 120, nullable: false),
            Phone = table.Column<string>("nvarchar(20)", maxLength: 20, nullable: false),
            Province = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            City = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            Address = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
            PostalCode = table.Column<string>("nvarchar(10)", maxLength: 10, nullable: true),
            CustomerNote = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
            Carrier = table.Column<string>("nvarchar(120)", maxLength: 120, nullable: true),
            TrackingNumber = table.Column<string>("nvarchar(120)", maxLength: 120, nullable: true),
            ShippedAt = table.Column<DateTime>("datetime2", nullable: true),
            Total = table.Column<decimal>("decimal(18,2)", nullable: false),
            Status = table.Column<int>("int", nullable: false, defaultValue: 0),
            PaymentStatus = table.Column<int>("int", nullable: false, defaultValue: 0),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Orders", x => x.Id));

        migrationBuilder.CreateTable("Vehicles", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Make = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            Model = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            YearFrom = table.Column<int>("int", nullable: false),
            YearTo = table.Column<int>("int", nullable: true),
            Engine = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            Trim = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
            Slug = table.Column<string>("nvarchar(180)", maxLength: 180, nullable: false),
            IsActive = table.Column<bool>("bit", nullable: false),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Vehicles", x => x.Id));

        migrationBuilder.CreateTable("ProductCategory", table => new
        {
            ProductId = table.Column<int>("int", nullable: false),
            CategoryId = table.Column<int>("int", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ProductCategory", x => new { x.ProductId, x.CategoryId });
            table.ForeignKey("FK_ProductCategory_Categories_CategoryId", x => x.CategoryId, "Categories", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_ProductCategory_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("OrderItems", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            OrderId = table.Column<int>("int", nullable: false),
            ProductId = table.Column<int>("int", nullable: false),
            ProductName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
            UnitPrice = table.Column<decimal>("decimal(18,2)", nullable: false),
            Quantity = table.Column<int>("int", nullable: false),
            LineTotal = table.Column<decimal>("decimal(18,2)", nullable: false),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_OrderItems", x => x.Id);
            table.ForeignKey("FK_OrderItems_Orders_OrderId", x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("OrderStatusHistory", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            OrderId = table.Column<int>("int", nullable: false),
            Status = table.Column<int>("int", nullable: false),
            Note = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: true),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
            table.ForeignKey("FK_OrderStatusHistory_Orders_OrderId", x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("PaymentTransactions", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            OrderId = table.Column<int>("int", nullable: false),
            Gateway = table.Column<string>("nvarchar(40)", maxLength: 40, nullable: false),
            Authority = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: true),
            ReferenceId = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: true),
            Amount = table.Column<decimal>("decimal(18,2)", nullable: false),
            Status = table.Column<int>("int", nullable: false, defaultValue: 0),
            GatewayCode = table.Column<int>("int", nullable: true),
            FailureReason = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: true),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
            table.ForeignKey("FK_PaymentTransactions_Orders_OrderId", x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("ProductCompatibilities", table => new
        {
            Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            ProductId = table.Column<int>("int", nullable: false),
            VehicleId = table.Column<int>("int", nullable: false),
            RequiresVinCheck = table.Column<bool>("bit", nullable: false),
            Note = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: true),
            CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ProductCompatibilities", x => x.Id);
            table.ForeignKey("FK_ProductCompatibilities_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_ProductCompatibilities_Vehicles_VehicleId", x => x.VehicleId, "Vehicles", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_Products_OemPartNumber", "Products", "OemPartNumber");
        migrationBuilder.CreateIndex("IX_Products_TechnicalPartNumber", "Products", "TechnicalPartNumber");
        migrationBuilder.CreateIndex("IX_Orders_Number", "Orders", "Number", unique: true);
        migrationBuilder.CreateIndex("IX_Orders_PublicToken", "Orders", "PublicToken", unique: true);
        migrationBuilder.CreateIndex("IX_OrderItems_OrderId", "OrderItems", "OrderId");
        migrationBuilder.CreateIndex("IX_OrderItems_ProductId", "OrderItems", "ProductId");
        migrationBuilder.CreateIndex("IX_OrderStatusHistory_OrderId_CreatedAt", "OrderStatusHistory", new[] { "OrderId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_PaymentTransactions_Authority", "PaymentTransactions", "Authority", unique: true, filter: "[Authority] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_PaymentTransactions_OrderId", "PaymentTransactions", "OrderId");
        migrationBuilder.CreateIndex("IX_ProductCategory_CategoryId", "ProductCategory", "CategoryId");
        migrationBuilder.CreateIndex("IX_ProductCompatibilities_ProductId_VehicleId", "ProductCompatibilities", new[] { "ProductId", "VehicleId" }, unique: true);
        migrationBuilder.CreateIndex("IX_ProductCompatibilities_VehicleId", "ProductCompatibilities", "VehicleId");
        migrationBuilder.CreateIndex("IX_Vehicles_Slug", "Vehicles", "Slug", unique: true);
        migrationBuilder.CreateIndex("IX_Vehicles_Make_Model_YearFrom_Engine_Trim", "Vehicles", new[] { "Make", "Model", "YearFrom", "Engine", "Trim" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("OrderItems");
        migrationBuilder.DropTable("OrderStatusHistory");
        migrationBuilder.DropTable("PaymentTransactions");
        migrationBuilder.DropTable("ProductCategory");
        migrationBuilder.DropTable("ProductCompatibilities");
        migrationBuilder.DropTable("Orders");
        migrationBuilder.DropTable("Vehicles");
        migrationBuilder.DropIndex("IX_Products_OemPartNumber", "Products");
        migrationBuilder.DropIndex("IX_Products_TechnicalPartNumber", "Products");
        migrationBuilder.DropColumn("AlternatePartNumbers", "Products");
        migrationBuilder.DropColumn("Brand", "Products");
        migrationBuilder.DropColumn("Manufacturer", "Products");
        migrationBuilder.DropColumn("OemPartNumber", "Products");
        migrationBuilder.DropColumn("TechnicalPartNumber", "Products");
    }
}
