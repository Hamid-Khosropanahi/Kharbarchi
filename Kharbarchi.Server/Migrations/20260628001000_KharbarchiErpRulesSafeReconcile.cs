using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    [Migration("20260628001000_KharbarchiErpRulesSafeReconcile")]
    public partial class KharbarchiErpRulesSafeReconcile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `KHB_Product_Price_History` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ProductSourceKey` CHAR(64) NOT NULL,
    `ProductName` VARCHAR(700) NULL,
    `SKU` VARCHAR(191) NULL,
    `ProductType` VARCHAR(80) NOT NULL,
    `PackageGroup` VARCHAR(50) NULL,
    `PackageCode` VARCHAR(50) NULL,
    `PriceType` VARCHAR(80) NOT NULL,
    `PriceAmount` DECIMAL(18,2) NOT NULL,
    `CurrencyCode` VARCHAR(20) NOT NULL DEFAULT 'TOMAN',
    `ValidFromUtc` DATETIME(6) NOT NULL,
    `ValidToUtc` DATETIME(6) NULL,
    `IsCurrent` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    KEY `IX_KHB_Product_Price_History_Product` (`ProductSourceKey`, `ProductType`, `PriceType`, `IsCurrent`),
    KEY `IX_KHB_Product_Price_History_SKU` (`SKU`),
    KEY `IX_KHB_Product_Price_History_Date` (`ValidFromUtc`, `ValidToUtc`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            AddColumnIfMissing(migrationBuilder, "KHB_Product_Final", "SaleMode", "VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "KHB_Product_Final", "PriceCalculationBasis", "VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "khb_sale_products", "SaleMode", "VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "khb_sale_products", "PriceCalculationBasis", "VARCHAR(80) NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables or columns.
        }

        private static void AddColumnIfMissing(MigrationBuilder migrationBuilder, string tableName, string columnName, string columnDefinition)
        {
            var tableValue = tableName.Replace("'", "''");
            var columnValue = columnName.Replace("'", "''");
            var ddlValue = columnDefinition.Replace("'", "''");
            migrationBuilder.Sql($@"
SET @ddl := (
    SELECT IF(
        COUNT(*) = 0,
        'ALTER TABLE `{tableValue}` ADD COLUMN `{columnValue}` {ddlValue}',
        'SELECT 1'
    )
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableValue}'
      AND COLUMN_NAME = '{columnValue}'
);");
            migrationBuilder.Sql("PREPARE stmt FROM @ddl;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }
    }
}
