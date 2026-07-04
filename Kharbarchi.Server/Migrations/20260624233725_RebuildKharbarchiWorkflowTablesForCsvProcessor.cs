using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class RebuildKharbarchiWorkflowTablesForCsvProcessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Product_Update_Queue`;");
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Product_Final`;");
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Package_Type`;");
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Commodity`;");
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Category_Map`;");
            migrationBuilder.Sql("-- KHB-SAFE: DROP TABLE IF EXISTS `KHB_Source_Product`;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_source_product` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,

    `SourceKey` VARCHAR(500) NOT NULL,
    `SourceRowId` BIGINT NULL,
    `SourceTableRowId` BIGINT NULL,
    `SourceRowHash` CHAR(64) NULL,
    `RawJson` LONGTEXT NULL,

    `RowNumber` INT NULL,

    `ProductName` VARCHAR(700) NULL,
    `ProductEnglishName` VARCHAR(700) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `ProductNameFa` VARCHAR(700) NULL,
    `ProductNameEn` VARCHAR(700) NULL,

    `MainProductName` VARCHAR(500) NULL,
    `MainProductEnglishName` VARCHAR(500) NULL,
    `MainProductSlug` VARCHAR(500) NULL,
    `MainProductNameFa` VARCHAR(500) NULL,
    `MainProductNameEn` VARCHAR(500) NULL,

    `GroupName` VARCHAR(500) NULL,
    `CategoryName` VARCHAR(500) NULL,
    `CategoryNameFa` VARCHAR(500) NULL,
    `CategoryNameEn` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `EnTaxonomic` VARCHAR(500) NULL,

    `BrandName` VARCHAR(300) NULL,
    `BrandEnglishName` VARCHAR(300) NULL,
    `BrandNameFa` VARCHAR(300) NULL,
    `BrandNameEn` VARCHAR(300) NULL,

    `PackageName` VARCHAR(300) NULL,
    `PackageOne` VARCHAR(300) NULL,
    `PackageOneKg` DECIMAL(18,6) NULL,
    `PackageCode` VARCHAR(80) NULL,
    `PackageGroup` VARCHAR(80) NULL,
    `UnitWeight` DECIMAL(18,6) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `CartonQuantity` INT NULL,
    `HaveOtherPackage` TINYINT(1) NULL,

    `PackagingPricePerPack` BIGINT NULL,
    `PackagingPricePerPackToman` BIGINT NULL,

    `KgCashPrice` BIGINT NULL,
    `KgCreditPrice` BIGINT NULL,
    `KgCashPriceInput` BIGINT NULL,
    `KgCreditPriceInput` BIGINT NULL,
    `KgCashPriceToman` BIGINT NULL,
    `KgCreditPriceToman` BIGINT NULL,

    `SalePriceCash` BIGINT NULL,
    `SalePriceInstallment` BIGINT NULL,
    `SalePriceCashToman` BIGINT NULL,
    `SalePriceCreditToman` BIGINT NULL,

    `PurchasePriceCash` BIGINT NULL,
    `PurchasePriceInstallment` BIGINT NULL,
    `PurchasePriceCashToman` BIGINT NULL,
    `PurchasePriceCreditToman` BIGINT NULL,

    `PriceInputUnit` VARCHAR(20) NULL,

    `ShortDescription` LONGTEXT NULL,
    `FullDescription` LONGTEXT NULL,
    `ImageUrl` LONGTEXT NULL,
    `GalleryJson` LONGTEXT NULL,
    `Status` VARCHAR(100) NULL,
    `WooProductId` BIGINT NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Source_Product_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Source_Product_ProductName` (`ProductName`(191)),
    KEY `IX_KHB_Source_Product_ProductNameFa` (`ProductNameFa`(191)),
    KEY `IX_KHB_Source_Product_CategorySlug` (`CategorySlug`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_category_map` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,

    `WooCategoryId` BIGINT NULL,

    `CategoryName` VARCHAR(500) NULL,
    `CategoryNameFa` VARCHAR(500) NULL,
    `CategoryNameEn` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `EnTaxonomic` VARCHAR(500) NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Category_Map_CategorySlug` (`CategorySlug`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_commodity` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,

    `WooCommodityId` BIGINT NULL,
    `CategoryId` BIGINT NULL,

    `CommodityName` VARCHAR(500) NULL,
    `CommodityEnglishName` VARCHAR(500) NULL,
    `CommodityNameFa` VARCHAR(500) NULL,
    `CommodityNameEn` VARCHAR(500) NULL,
    `CommoditySlug` VARCHAR(500) NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Commodity_CategoryId` (`CategoryId`),
    KEY `IX_KHB_Commodity_Slug` (`CommoditySlug`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_package_type` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,

    `WooPackageId` BIGINT NULL,

    `PackageName` VARCHAR(500) NULL,
    `PackageEnglishName` VARCHAR(500) NULL,
    `PackageNameFa` VARCHAR(500) NULL,
    `PackageNameEn` VARCHAR(500) NULL,

    `PackageCode` VARCHAR(80) NULL,
    `PackageGroup` VARCHAR(80) NULL,

    `UnitWeight` DECIMAL(18,6) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,

    `PacksPerCarton` INT NULL,
    `PackagingPricePerPack` BIGINT NULL,
    `PackagingPricePerPackToman` BIGINT NULL,

    `ImageTag` VARCHAR(200) NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Package_Type_Code` (`PackageCode`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_final` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,

    `SourceKey` VARCHAR(500) NOT NULL,
    `SourceRowId` BIGINT NULL,
    `SourceTableRowId` BIGINT NULL,
    `SourceRowHash` CHAR(64) NULL,

    `WooProductId` BIGINT NULL,

    `CategoryId` BIGINT NULL,
    `CommodityId` BIGINT NULL,
    `PackageTypeId` BIGINT NULL,

    `ProductName` VARCHAR(700) NULL,
    `ProductEnglishName` VARCHAR(700) NULL,
    `ProductNameFa` VARCHAR(700) NULL,
    `ProductNameEn` VARCHAR(700) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `SKU` VARCHAR(191) NULL,

    `BrandName` VARCHAR(300) NULL,
    `BrandEnglishName` VARCHAR(300) NULL,
    `BrandNameFa` VARCHAR(300) NULL,
    `BrandNameEn` VARCHAR(300) NULL,

    `PackageName` VARCHAR(500) NULL,
    `PackageEnglishName` VARCHAR(500) NULL,
    `PackageNameFa` VARCHAR(500) NULL,
    `PackageNameEn` VARCHAR(500) NULL,
    `PackageGroup` VARCHAR(80) NULL,
    `PackageCode` VARCHAR(80) NULL,

    `UnitWeight` DECIMAL(18,6) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,

    `PackagingPricePerPack` BIGINT NULL,
    `PackagingPricePerPackToman` BIGINT NULL,

    `KgCashPrice` BIGINT NULL,
    `KgCreditPrice` BIGINT NULL,
    `KgCashPriceToman` BIGINT NULL,
    `KgCreditPriceToman` BIGINT NULL,

    `SalePriceCash` BIGINT NULL,
    `SalePriceInstallment` BIGINT NULL,
    `SalePriceCashToman` BIGINT NULL,
    `SalePriceCreditToman` BIGINT NULL,

    `PurchasePriceCash` BIGINT NULL,
    `PurchasePriceInstallment` BIGINT NULL,
    `PurchasePriceCashToman` BIGINT NULL,
    `PurchasePriceCreditToman` BIGINT NULL,

    `PriceInputUnit` VARCHAR(20) NULL,

    `ShortDescription` LONGTEXT NULL,
    `FullDescription` LONGTEXT NULL,
    `ImageUrl` LONGTEXT NULL,
    `GalleryJson` LONGTEXT NULL,

    `Status` VARCHAR(100) NOT NULL DEFAULT 'draft',
    `CatalogVisibility` VARCHAR(100) NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Product_Final_SKU` (`SKU`),
    KEY `IX_KHB_Product_Final_CategoryId` (`CategoryId`),
    KEY `IX_KHB_Product_Final_CommodityId` (`CommodityId`),
    KEY `IX_KHB_Product_Final_PackageTypeId` (`PackageTypeId`),
    KEY `IX_KHB_Product_Final_ProductSlug` (`ProductSlug`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_update_queue` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,

    `EntityType` VARCHAR(80) NOT NULL,
    `SourceKey` VARCHAR(500) NOT NULL,
    `ExternalKey` VARCHAR(500) NULL,

    `PayloadJson` LONGTEXT NULL,

    `SyncStatus` VARCHAR(80) NOT NULL DEFAULT 'pending',
    `TryCount` INT NOT NULL DEFAULT 0,
    `LastError` LONGTEXT NULL,
    `WooObjectId` BIGINT NULL,

    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Product_Update_Queue_SourceKey` (`SourceKey`),
    KEY `IX_KHB_Product_Update_Queue_Status` (`SyncStatus`),
    KEY `IX_KHB_Product_Update_Queue_EntityType` (`EntityType`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
