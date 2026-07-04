using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileAppDbContextModel20260703 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "gnr_Commodities", "WooCommerceCommodityId", "`WooCommerceCommodityId` BIGINT NULL");
            AddColumnIfMissing(migrationBuilder, "gnr_Categories", "WooCommerceCategoryId", "`WooCommerceCategoryId` BIGINT NULL");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_productwoocontrolprofiles` (
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
    CONSTRAINT `PK_khb_productwoocontrolprofiles` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_khb_productwoocontrolprofiles_gnr_Products_ProductId`
        FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE,
    UNIQUE KEY `IX_khb_productwoocontrolprofiles_ProductId` (`ProductId`),
    KEY `IX_khb_productwoocontrolprofiles_PriceCheckStatus` (`PriceCheckStatus`),
    KEY `IX_khb_productwoocontrolprofiles_WooSyncStatus` (`WooSyncStatus`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_price_history` (
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

            AddColumnIfMissing(migrationBuilder, "khb_product_final", "SaleMode", "`SaleMode` VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "khb_product_final", "PriceCalculationBasis", "`PriceCalculationBasis` VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "khb_sale_products", "SaleMode", "`SaleMode` VARCHAR(80) NULL");
            AddColumnIfMissing(migrationBuilder, "khb_sale_products", "PriceCalculationBasis", "`PriceCalculationBasis` VARCHAR(80) NULL");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "0c5e0418-46b3-4c6e-887e-0c182171ab11");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "4f43b487-3f8e-426d-9a46-048c7d07f7f9");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "5f36c2f9-330a-492c-8ebf-65141782f2bb");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "6240e185-5c3a-410b-99d3-9767571fdf24");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "67320cb2-92a2-4de7-971b-7e9e80244f4b");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "e572b070-82bd-47f0-b486-cc1b644b2d9e");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e");

            AddIndexIfMissing(
                migrationBuilder,
                "gnr_Commodities",
                "IX_gnr_Commodities_WooCommerceCommodityId",
                "`WooCommerceCommodityId`",
                unique: true);
            AddIndexIfMissing(
                migrationBuilder,
                "gnr_Categories",
                "IX_gnr_Categories_WooCommerceCategoryId",
                "`WooCommerceCategoryId`",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
#if false
            migrationBuilder.DropTable(
                name: "khb_productwoocontrolprofiles");

            migrationBuilder.DropIndex(
                name: "IX_gnr_Commodities_WooCommerceCommodityId",
                table: "gnr_Commodities");

            migrationBuilder.DropIndex(
                name: "IX_gnr_Categories_WooCommerceCategoryId",
                table: "gnr_Categories");

            migrationBuilder.DropColumn(
                name: "WooCommerceCommodityId",
                table: "gnr_Commodities");

            migrationBuilder.DropColumn(
                name: "WooCommerceCategoryId",
                table: "gnr_Categories");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "215157e4-72b1-42a6-9cd4-c6a3dcc6b93b");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "77a2d31b-1d2a-40ee-8922-a200fa2112ad");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "8f11bd20-0686-404e-a8bd-972f529a734b");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "3bde4074-a6da-40d1-8a77-0afad83243f8");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "e6d026c9-fa74-4b41-bf56-0553e18cf822");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "6ff280c3-a24e-4c13-9f7b-b0db96d608b3");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "b1c50a24-2662-4e1c-ae24-81dc5379b09d");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "b1e46921-b59f-4923-beaf-6331633f4537");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "bcd93de1-7b57-4807-923a-3027dfaa4204");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "7d0aeabf-7140-431b-9549-2f4ea5c0d3c8");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "9a8fbf6f-d0c2-436a-9f35-452d761ed594");
#endif
            // KHB-SAFE: rollback intentionally preserves data-bearing tables, columns, and indexes.
        }

        private static void AddColumnIfMissing(
            MigrationBuilder migrationBuilder,
            string tableName,
            string columnName,
            string columnDefinition)
        {
            var tableValue = tableName.Replace("'", "''");
            var columnValue = columnName.Replace("'", "''");
            var ddlValue = columnDefinition.Replace("'", "''");
            migrationBuilder.Sql($@"
SET @ddl := (
    SELECT IF(
        EXISTS (
            SELECT 1
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = '{tableValue}'
        )
        AND NOT EXISTS (
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = '{tableValue}'
              AND COLUMN_NAME = '{columnValue}'
        ),
        'ALTER TABLE `{tableValue}` ADD COLUMN {ddlValue}',
        'SELECT 1'
    )
);");
            migrationBuilder.Sql("PREPARE stmt FROM @ddl;");
            migrationBuilder.Sql("EXECUTE stmt;");
            migrationBuilder.Sql("DEALLOCATE PREPARE stmt;");
        }

        private static void AddIndexIfMissing(
            MigrationBuilder migrationBuilder,
            string tableName,
            string indexName,
            string columnExpression,
            bool unique)
        {
            var tableValue = tableName.Replace("'", "''");
            var indexValue = indexName.Replace("'", "''");
            var uniqueness = unique ? "UNIQUE " : string.Empty;
            var ddlValue = $"CREATE {uniqueness}INDEX `{indexName}` ON `{tableName}` ({columnExpression})"
                .Replace("'", "''");
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
