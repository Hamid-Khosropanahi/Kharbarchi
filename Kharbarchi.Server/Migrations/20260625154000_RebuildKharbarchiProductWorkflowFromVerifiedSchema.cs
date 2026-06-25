using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations;

[Migration("20260625154000_RebuildKharbarchiProductWorkflowFromVerifiedSchema")]
public partial class RebuildKharbarchiProductWorkflowFromVerifiedSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `all_product_with_process` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ImportBatchId` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SourceRowNumber` int DEFAULT NULL,
  `SourceRowHash` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `RawJson` longtext COLLATE utf8mb4_unicode_ci,
  `MainProductName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MainProductSlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `GroupName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategoryName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategorySlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductEnglishName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductSlug` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SKU` varchar(191) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BrandName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitWeight` decimal(18,6) DEFAULT NULL,
  `PacksPerCarton` int DEFAULT NULL,
  `CartonQuantity` int DEFAULT NULL,
  `SalePriceCash` decimal(18,2) DEFAULT NULL,
  `SalePriceInstallment` decimal(18,2) DEFAULT NULL,
  `PurchasePriceCash` decimal(18,2) DEFAULT NULL,
  `PurchasePriceInstallment` decimal(18,2) DEFAULT NULL,
  `ShortDescription` longtext COLLATE utf8mb4_unicode_ci,
  `FullDescription` longtext COLLATE utf8mb4_unicode_ci,
  `ImageUrl` longtext COLLATE utf8mb4_unicode_ci,
  `GalleryJson` longtext COLLATE utf8mb4_unicode_ci,
  `Status` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WooProductId` bigint DEFAULT NULL,
  `HaveOtherPackage` tinyint(1) DEFAULT NULL,
  `PackageOne` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `BrandEnglishName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackagingPricePerPack` decimal(18,2) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_All_Product_With_Process_SourceRowHash` (`SourceRowHash`),
  KEY `IX_All_Product_With_Process_ProductName` (`ProductName`(191)),
  KEY `IX_All_Product_With_Process_MainProductName` (`MainProductName`(191)),
  KEY `IX_All_Product_With_Process_SKU` (`SKU`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_product_main_groups` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MainProductName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MainProductSlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategoryName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategorySlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Description` longtext COLLATE utf8mb4_unicode_ci,
  `ImageUrl` longtext COLLATE utf8mb4_unicode_ci,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SourceKey` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Name` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `EnTaxonomic` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_product_main_groups_slug` (`MainProductSlug`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_sale_products` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MainGroupId` bigint DEFAULT NULL,
  `SourceRowHash` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `WooProductId` bigint DEFAULT NULL,
  `ProductName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductEnglishName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductSlug` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SKU` varchar(191) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BrandName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BrandEnglishName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackagingGroup` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageCode` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitWeight` decimal(18,6) DEFAULT NULL,
  `PacksPerCarton` int DEFAULT NULL,
  `CartonQuantity` int DEFAULT NULL,
  `PackagingPricePerPack` decimal(18,2) DEFAULT NULL,
  `KgPriceCash` decimal(18,2) DEFAULT NULL,
  `KgPriceInstallment` decimal(18,2) DEFAULT NULL,
  `SalePriceCash` decimal(18,2) DEFAULT NULL,
  `SalePriceInstallment` decimal(18,2) DEFAULT NULL,
  `PurchasePriceCash` decimal(18,2) DEFAULT NULL,
  `PurchasePriceInstallment` decimal(18,2) DEFAULT NULL,
  `ShortDescription` longtext COLLATE utf8mb4_unicode_ci,
  `FullDescription` longtext COLLATE utf8mb4_unicode_ci,
  `ImageUrl` longtext COLLATE utf8mb4_unicode_ci,
  `GalleryJson` longtext COLLATE utf8mb4_unicode_ci,
  `Status` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'draft',
  `RawJson` longtext COLLATE utf8mb4_unicode_ci,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_sale_products_hash` (`SourceRowHash`),
  KEY `IX_khb_sale_products_woo` (`WooProductId`),
  KEY `IX_khb_sale_products_sku` (`SKU`),
  KEY `IX_khb_sale_products_name` (`ProductName`(191))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_source_product` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `SourceRowId` bigint DEFAULT NULL,
  `ProductName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductEnglishName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MainProductName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategoryName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategorySlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BrandName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BrandEnglishName` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageOne` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitWeightKg` decimal(18,6) DEFAULT NULL,
  `KgCashPrice` decimal(18,2) DEFAULT NULL,
  `KgCreditPrice` decimal(18,2) DEFAULT NULL,
  `RawJson` longtext COLLATE utf8mb4_unicode_ci,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Source_Product_SourceKey` (`SourceKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_category_map` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CategoryName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategorySlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WooCategoryId` bigint DEFAULT NULL,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_commodity` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CommodityName` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CommoditySlug` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WooCommodityId` bigint DEFAULT NULL,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_package_type` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PackageGroup` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageCode` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageTitle` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitWeightKg` decimal(18,6) DEFAULT NULL,
  `PacksPerCarton` int DEFAULT NULL,
  `PackagingPricePerPack` decimal(18,2) DEFAULT NULL,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_product_final` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `MainGroupId` bigint DEFAULT NULL,
  `CategorySourceKey` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CommoditySourceKey` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageSourceKey` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductEnglishName` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductSlug` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SKU` varchar(191) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageGroup` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PackageCode` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitWeightKg` decimal(18,6) DEFAULT NULL,
  `PacksPerCarton` int DEFAULT NULL,
  `PackagingPricePerPack` decimal(18,2) DEFAULT NULL,
  `KgCashPrice` decimal(18,2) DEFAULT NULL,
  `KgCreditPrice` decimal(18,2) DEFAULT NULL,
  `SaleCashPrice` decimal(18,2) DEFAULT NULL,
  `SaleCreditPrice` decimal(18,2) DEFAULT NULL,
  `BuyCashPrice` decimal(18,2) DEFAULT NULL,
  `BuyCreditPrice` decimal(18,2) DEFAULT NULL,
  `Status` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CatalogVisibility` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WooPayloadJson` longtext COLLATE utf8mb4_unicode_ci,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Final_SKU` (`SKU`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
CREATE TABLE IF NOT EXISTS `khb_product_update_queue` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SourceKey` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EntityType` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'product',
  `QueueStatus` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
  `ActionType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'upsert',
  `SKU` varchar(191) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ProductSlug` varchar(700) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WooProductId` bigint DEFAULT NULL,
  `WooPayloadJson` longtext COLLATE utf8mb4_unicode_ci,
  `LastError` longtext COLLATE utf8mb4_unicode_ci,
  `CreatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Update_Queue_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Update_Queue_Status` (`QueueStatus`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
""");

        migrationBuilder.Sql(@"""
ALTER TABLE `khb_product_update_queue`
    MODIFY COLUMN `EntityType` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'product';
""");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_product_update_queue`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_product_final`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_package_type`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_commodity`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_category_map`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_source_product`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_sale_products`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `khb_product_main_groups`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `all_product_with_process`;");
    }
}
