using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddErpCustomersCreditSalesWorkflow20260721 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserName",
                table: "com_orders",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddressLine",
                table: "com_orders",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCity",
                table: "com_orders",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryPostalCode",
                table: "com_orders",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "com_orders",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "com_orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscount",
                table: "com_orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineDiscount",
                table: "com_orderitems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalUnitPrice",
                table: "com_orderitems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "com_orderitems",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "cbi_customers",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "cbi_customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CreditPlanTitle",
                table: "cbi_customers",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreditReceivedAtUtc",
                table: "cbi_customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistributionStatus",
                table: "cbi_customers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "cbi_customers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreditBlocked",
                table: "cbi_customers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalEntity",
                table: "cbi_customers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastImportedAtUtc",
                table: "cbi_customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalEntityId",
                table: "cbi_customers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "cbi_customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsedCredit",
                table: "cbi_customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Preserve meaningful snapshots for rows that existed before this ERP workflow.
            migrationBuilder.Sql("""
                UPDATE com_orders o
                INNER JOIN cbi_customers c ON c.Id = o.CustomerId
                SET o.GrossAmount = o.TotalAmount,
                    o.DeliveryAddressLine = COALESCE(c.AddressLine, ''),
                    o.DeliveryCity = COALESCE(c.City, ''),
                    o.DeliveryPostalCode = COALESCE(c.PostalCode, ''),
                    o.CreatedByUserName = 'migration-existing'
                WHERE o.CreatedByUserName = '';
                """);

            migrationBuilder.Sql("""
                UPDATE com_orderitems oi
                INNER JOIN gnr_products p ON p.Id = oi.ProductId
                SET oi.ProductName = p.Name,
                    oi.OriginalUnitPrice = oi.UnitPrice
                WHERE oi.ProductName = '';
                """);

            migrationBuilder.CreateTable(
                name: "cbi_customercredithistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PreviousCreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewCreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NewBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Source = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ChangedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cbi_customercredithistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cbi_customercredithistory_cbi_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "cbi_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prc_productpricehistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    PriceType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Source = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ChangedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prc_productpricehistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prc_productpricehistory_gnr_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prc_productpricehistory_gnr_productvariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "gnr_productvariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "sec_aspnetroles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b52865dd-a178-45a1-82ae-59f2e75c9c17", "b52865dd-a178-45a1-82ae-59f2e75c9c17", "Seller", "SELLER" });

            migrationBuilder.CreateIndex(
                name: "IX_cbi_customers_LegalEntityId",
                table: "cbi_customers",
                column: "LegalEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cbi_customers_PhoneNumber",
                table: "cbi_customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_cbi_customercredithistory_CustomerId_ChangedAtUtc",
                table: "cbi_customercredithistory",
                columns: new[] { "CustomerId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_prc_productpricehistory_ProductId_ProductVariantId_PriceType~",
                table: "prc_productpricehistory",
                columns: new[] { "ProductId", "ProductVariantId", "PriceType", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_prc_productpricehistory_ProductVariantId",
                table: "prc_productpricehistory",
                column: "ProductVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cbi_customercredithistory");

            migrationBuilder.DropTable(
                name: "prc_productpricehistory");

            migrationBuilder.DropIndex(
                name: "IX_cbi_customers_LegalEntityId",
                table: "cbi_customers");

            migrationBuilder.DropIndex(
                name: "IX_cbi_customers_PhoneNumber",
                table: "cbi_customers");

            migrationBuilder.DeleteData(
                table: "sec_aspnetroles",
                keyColumn: "Id",
                keyValue: "b52865dd-a178-45a1-82ae-59f2e75c9c17");

            migrationBuilder.DropColumn(
                name: "CreatedByUserName",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddressLine",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "DeliveryCity",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "DeliveryPostalCode",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "TotalDiscount",
                table: "com_orders");

            migrationBuilder.DropColumn(
                name: "LineDiscount",
                table: "com_orderitems");

            migrationBuilder.DropColumn(
                name: "OriginalUnitPrice",
                table: "com_orderitems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "com_orderitems");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "CreditPlanTitle",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "CreditReceivedAtUtc",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "DistributionStatus",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "IsCreditBlocked",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "IsLegalEntity",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "LastImportedAtUtc",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "LegalEntityId",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "UsedCredit",
                table: "cbi_customers");
        }
    }
}
