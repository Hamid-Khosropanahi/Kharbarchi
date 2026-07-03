using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateKharbarchiProductWorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Product_Update_Queue`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Product_Final`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Package_Type`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Commodity`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Category_Map`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Source_Product`;");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Source_Product` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `SourceRowHash` CHAR(64) NULL,
    `SourceTableRowId` BIGINT NULL,
    `ProductNameFa` VARCHAR(700) NULL,
    `ProductNameEn` VARCHAR(700) NULL,
    `MainProductNameFa` VARCHAR(500) NULL,
    `MainProductNameEn` VARCHAR(500) NULL,
    `CategoryNameFa` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `BrandNameFa` VARCHAR(300) NULL,
    `BrandNameEn` VARCHAR(300) NULL,
    `PackageOne` VARCHAR(300) NULL,
    `PackageOneKg` DECIMAL(18,6) NULL,
    `HaveOtherPackage` TINYINT(1) NULL,
    `KgCashPriceInput` BIGINT NULL,
    `KgCreditPriceInput` BIGINT NULL,
    `KgCashPriceToman` BIGINT NULL,
    `KgCreditPriceToman` BIGINT NULL,
    `PriceInputUnit` VARCHAR(20) NULL,
    `RawJson` LONGTEXT NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Source_Product_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Category_Map` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `WooCategoryId` BIGINT NULL,
    `CategoryNameFa` VARCHAR(500) NULL,
    `CategoryNameEn` VARCHAR(500) NULL,
    `CategorySlug` VARCHAR(500) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`),
    UNIQUE KEY `UX_KHB_Category_Map_CategorySlug` (`CategorySlug`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Commodity` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `WooCommodityId` BIGINT NULL,
    `CategoryId` BIGINT NULL,
    `CommodityNameFa` VARCHAR(500) NULL,
    `CommodityNameEn` VARCHAR(500) NULL,
    `CommoditySlug` VARCHAR(500) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`),
    UNIQUE KEY `UX_KHB_Commodity_Slug` (`CommoditySlug`),
    KEY `IX_KHB_Commodity_CategoryId` (`CategoryId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Package_Type` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `WooPackageId` BIGINT NULL,
    `PackageNameFa` VARCHAR(500) NULL,
    `PackageNameEn` VARCHAR(500) NULL,
    `PackageCode` VARCHAR(80) NULL,
    `PackageGroup` VARCHAR(80) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `PackagingPricePerPackToman` BIGINT NULL,
    `ImageTag` VARCHAR(200) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`),
    UNIQUE KEY `UX_KHB_Package_Type_Code` (`PackageCode`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Product_Final` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NOT NULL,
    `SourceRowHash` CHAR(64) NULL,
    `SourceRowId` BIGINT NULL,
    `WooProductId` BIGINT NULL,
    `CategoryId` BIGINT NULL,
    `CommodityId` BIGINT NULL,
    `PackageTypeId` BIGINT NULL,
    `ProductNameFa` VARCHAR(700) NULL,
    `ProductNameEn` VARCHAR(700) NULL,
    `ProductSlug` VARCHAR(700) NULL,
    `SKU` VARCHAR(191) NULL,
    `BrandNameFa` VARCHAR(300) NULL,
    `BrandNameEn` VARCHAR(300) NULL,
    `PackageNameFa` VARCHAR(500) NULL,
    `PackageNameEn` VARCHAR(500) NULL,
    `PackageGroup` VARCHAR(80) NULL,
    `PackageCode` VARCHAR(80) NULL,
    `UnitWeightKg` DECIMAL(18,6) NULL,
    `PacksPerCarton` INT NULL,
    `PackagingPricePerPackToman` BIGINT NULL,
    `KgCashPriceToman` BIGINT NULL,
    `KgCreditPriceToman` BIGINT NULL,
    `SalePriceCashToman` BIGINT NULL,
    `SalePriceCreditToman` BIGINT NULL,
    `PurchasePriceCashToman` BIGINT NULL,
    `PurchasePriceCreditToman` BIGINT NULL,
    `PriceInputUnit` VARCHAR(20) NULL,
    `ShortDescription` LONGTEXT NULL,
    `FullDescription` LONGTEXT NULL,
    `ImageUrl` LONGTEXT NULL,
    `GalleryJson` LONGTEXT NULL,
    `Status` VARCHAR(100) NOT NULL DEFAULT 'draft',
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
    UNIQUE KEY `UX_KHB_Product_Final_SKU` (`SKU`),
    KEY `IX_KHB_Product_Final_CategoryId` (`CategoryId`),
    KEY `IX_KHB_Product_Final_CommodityId` (`CommodityId`),
    KEY `IX_KHB_Product_Final_PackageTypeId` (`PackageTypeId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE `KHB_Product_Update_Queue` (
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
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Product_Update_Queue`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Product_Final`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Package_Type`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Commodity`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Category_Map`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `KHB_Source_Product`;");
        }
    }
}
