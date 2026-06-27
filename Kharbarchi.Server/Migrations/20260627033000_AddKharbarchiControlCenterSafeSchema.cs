using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    public partial class AddKharbarchiControlCenterSafeSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "gnr_Categories", "WooCommerceCategoryId", "`WooCommerceCategoryId` BIGINT NULL");
            AddColumnIfMissing(migrationBuilder, "gnr_Commodities", "WooCommerceCommodityId", "`WooCommerceCommodityId` BIGINT NULL");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_ProductWooControlProfiles` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `ProductId` INT NOT NULL,
    `PriceSourceMode` VARCHAR(80) NOT NULL DEFAULT 'final_price',
    `PackageGroup` VARCHAR(50) NOT NULL DEFAULT 'none',
    `PackageCode` VARCHAR(80) NULL,
    `PackageTitle` VARCHAR(300) NULL,
    `ImageTag` VARCHAR(300) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `ProductCartonCount` INT NULL,
    `BulkWeightKg` DECIMAL(18,6) NULL,
    `MinPurchaseKg` DECIMAL(18,6) NULL,
    `MinCartons` INT NOT NULL DEFAULT 1,
    `MaxCartons` INT NOT NULL DEFAULT 0,
    `CartonStep` INT NOT NULL DEFAULT 1,
    `SaleUnit` VARCHAR(50) NOT NULL DEFAULT 'carton',
    `WoodmartPriceUnitOfMeasure` VARCHAR(80) NOT NULL DEFAULT 'کارتن',
    `SaleCashPrice` DECIMAL(18,2) NULL,
    `SaleCreditPrice` DECIMAL(18,2) NULL,
    `BuyCashPrice` DECIMAL(18,2) NULL,
    `BuyCreditPrice` DECIMAL(18,2) NULL,
    `SaleCashPricePerKg` DECIMAL(18,2) NULL,
    `SaleCreditPricePerKg` DECIMAL(18,2) NULL,
    `BuyCashPricePerKg` DECIMAL(18,2) NULL,
    `BuyCreditPricePerKg` DECIMAL(18,2) NULL,
    `ExpectedSaleCreditPrice` DECIMAL(18,2) NULL,
    `ExpectedSaleCashPrice` DECIMAL(18,2) NULL,
    `ExpectedBuyCreditPrice` DECIMAL(18,2) NULL,
    `ExpectedBuyCashPrice` DECIMAL(18,2) NULL,
    `SaleCreditDiff` DECIMAL(18,2) NULL,
    `SaleCashDiff` DECIMAL(18,2) NULL,
    `BuyCreditDiff` DECIMAL(18,2) NULL,
    `BuyCashDiff` DECIMAL(18,2) NULL,
    `PriceCheckStatus` VARCHAR(20) NOT NULL DEFAULT 'red',
    `PriceCheckCode` VARCHAR(120) NOT NULL DEFAULT 'NEED_FIX',
    `PriceCheckNote` VARCHAR(2000) NULL,
    `PriceCheckPercent` DECIMAL(9,4) NULL,
    `PriceCheckAmount` DECIMAL(18,2) NULL,
    `NeedFix` TINYINT(1) NOT NULL DEFAULT 1,
    `AutoDraftRequired` TINYINT(1) NOT NULL DEFAULT 0,
    `WooSyncStatus` VARCHAR(50) NOT NULL DEFAULT 'pending',
    `WooLastError` VARCHAR(2000) NULL,
    `WooSyncedAtUtc` DATETIME(6) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NULL,
    CONSTRAINT `PK_khb_ProductWooControlProfiles` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_khb_ProductWooControlProfiles_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE,
    UNIQUE KEY `IX_khb_ProductWooControlProfiles_ProductId` (`ProductId`),
    KEY `IX_khb_ProductWooControlProfiles_PriceCheckStatus` (`PriceCheckStatus`),
    KEY `IX_khb_ProductWooControlProfiles_WooSyncStatus` (`WooSyncStatus`)
) CHARACTER SET utf8mb4;");

            AddIndexIfMissing(migrationBuilder, "gnr_Categories", "IX_gnr_Categories_WooCommerceCategoryId", "`WooCommerceCategoryId`");
            AddIndexIfMissing(migrationBuilder, "gnr_Commodities", "IX_gnr_Commodities_WooCommerceCommodityId", "`WooCommerceCommodityId`");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safe rollback intentionally does not drop data-bearing tables or columns.
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
        'ALTER TABLE `{tableValue}` ADD COLUMN {ddlValue}',
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

        private static void AddIndexIfMissing(MigrationBuilder migrationBuilder, string tableName, string indexName, string columnExpression)
        {
            var tableValue = tableName.Replace("'", "''");
            var indexValue = indexName.Replace("'", "''");
            var ddlValue = $"CREATE INDEX `{indexName}` ON `{tableName}` ({columnExpression})".Replace("'", "''");
            migrationBuilder.Sql($@"
SET @ddl := (
    SELECT IF(
        COUNT(*) = 0,
        '{ddlValue}',
        'SELECT 1'
    )
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableValue}'
      AND INDEX_NAME = '{indexValue}'
);");
            migrationBuilder.Sql("PREPARE stmt FROM @ddl;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }
    }
}
