using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    [Migration("20260624204500_RebuildHardcodedProductWorkflowTables")]
    public partial class RebuildHardcodedProductWorkflowTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Product_Update_Queue`;
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Product_Final`;
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Package_Type`;
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Commodity`;
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Category_Map`;
-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Source_Product`;
-- KHB-SAFE: DROP TABLE IF EXISTS `khb_sale_products`;
-- KHB-SAFE: DROP TABLE IF EXISTS `khb_product_main_groups`;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_product_main_groups` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `Name` VARCHAR(500) NULL,
    `MainProductName` VARCHAR(500) NULL,
    `MainProductSlug` VARCHAR(500) NULL,
    `CategoryName` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `EnTaxonomic` VARCHAR(500) NULL,
    `Description` LONGTEXT NULL,
    `ImageUrl` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_khb_product_main_groups_SourceKey` (`SourceKey`),
    KEY `IX_khb_product_main_groups_slug` (`MainProductSlug`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_sale_products` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `MainGroupId` BIGINT NULL,
    `SourceRowHash` CHAR(64) NOT NULL,
    `WooProductId` BIGINT NULL,
    `ProductName` VARCHAR(700) NULL,
    `ProductEnglishName` VARCHAR(700) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `SKU` VARCHAR(191) NULL,
    `BrandName` VARCHAR(300) NULL,
    `BrandEnglishName` VARCHAR(300) NULL,
    `PackageName` VARCHAR(300) NULL,
    `PackagingGroup` VARCHAR(50) NULL,
    `PackageCode` VARCHAR(50) NULL,
    `UnitWeight` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `CartonQuantity` INT NULL,
    `PackagingPricePerPack` DECIMAL(18,2) NULL,
    `KgPriceCash` DECIMAL(18,2) NULL,
    `KgPriceInstallment` DECIMAL(18,2) NULL,
    `SalePriceCash` DECIMAL(18,2) NULL,
    `SalePriceInstallment` DECIMAL(18,2) NULL,
    `PurchasePriceCash` DECIMAL(18,2) NULL,
    `PurchasePriceInstallment` DECIMAL(18,2) NULL,
    `ShortDescription` LONGTEXT NULL,
    `FullDescription` LONGTEXT NULL,
    `ImageUrl` LONGTEXT NULL,
    `GalleryJson` LONGTEXT NULL,
    `Status` VARCHAR(100) NOT NULL DEFAULT 'draft',
    `RawJson` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_khb_sale_products_hash` (`SourceRowHash`),
    KEY `IX_khb_sale_products_woo` (`WooProductId`),
    KEY `IX_khb_sale_products_sku` (`SKU`),
    KEY `IX_khb_sale_products_name` (`ProductName`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_source_product` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` CHAR(64) NOT NULL,
    `SourceRowId` BIGINT NULL,
    `ProductName` VARCHAR(700) NULL,
    `ProductEnglishName` VARCHAR(700) NULL,
    `MainProductName` VARCHAR(500) NULL,
    `CategoryName` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `BrandName` VARCHAR(300) NULL,
    `BrandEnglishName` VARCHAR(300) NULL,
    `PackageOne` VARCHAR(300) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `KgCashPrice` DECIMAL(18,2) NULL,
    `KgCreditPrice` DECIMAL(18,2) NULL,
    `RawJson` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Source_Product_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_category_map` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `CategoryName` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `WooCategoryId` BIGINT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_commodity` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `CommodityName` VARCHAR(500) NULL,
    `CommoditySlug` VARCHAR(500) NULL,
    `WooCommodityId` BIGINT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_package_type` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `PackageGroup` VARCHAR(50) NULL,
    `PackageCode` VARCHAR(50) NULL,
    `PackageTitle` VARCHAR(300) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `PackagingPricePerPack` DECIMAL(18,2) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_product_final` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` CHAR(64) NOT NULL,
    `MainGroupId` BIGINT NULL,
    `CategorySourceKey` VARCHAR(500) NULL,
    `CommoditySourceKey` VARCHAR(500) NULL,
    `PackageSourceKey` VARCHAR(500) NULL,
    `ProductName` VARCHAR(700) NULL,
    `ProductEnglishName` VARCHAR(700) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `SKU` VARCHAR(191) NULL,
    `PackageGroup` VARCHAR(50) NULL,
    `PackageCode` VARCHAR(50) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `PackagingPricePerPack` DECIMAL(18,2) NULL,
    `KgCashPrice` DECIMAL(18,2) NULL,
    `KgCreditPrice` DECIMAL(18,2) NULL,
    `SaleCashPrice` DECIMAL(18,2) NULL,
    `SaleCreditPrice` DECIMAL(18,2) NULL,
    `BuyCashPrice` DECIMAL(18,2) NULL,
    `BuyCreditPrice` DECIMAL(18,2) NULL,
    `Status` VARCHAR(100) NULL,
    `CatalogVisibility` VARCHAR(50) NULL,
    `WooPayloadJson` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Product_Final_SKU` (`SKU`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `khb_product_update_queue` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` CHAR(64) NOT NULL,
    `QueueStatus` VARCHAR(50) NOT NULL DEFAULT 'pending',
    `ActionType` VARCHAR(50) NOT NULL DEFAULT 'upsert',
    `SKU` VARCHAR(191) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `WooProductId` BIGINT NULL,
    `WooPayloadJson` LONGTEXT NULL,
    `LastError` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Product_Update_Queue_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Product_Update_Queue_Status` (`QueueStatus`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
