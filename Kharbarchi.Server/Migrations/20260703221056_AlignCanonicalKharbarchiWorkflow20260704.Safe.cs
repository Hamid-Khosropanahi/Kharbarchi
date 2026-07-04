using Microsoft.EntityFrameworkCore.Migrations;

namespace Kharbarchi.Server.Migrations;

public partial class AlignCanonicalKharbarchiWorkflow20260704
{
    private static void ApplySafeForwardOnlyMigration(MigrationBuilder migrationBuilder)
    {
        RenameMixedCaseTables(migrationBuilder);
        CreateCanonicalTables(migrationBuilder);
        AddCanonicalColumns(migrationBuilder);
        BackfillCanonicalColumns(migrationBuilder);
        AddCanonicalIndexes(migrationBuilder);
    }

    private static void RenameMixedCaseTables(MigrationBuilder migrationBuilder)
    {
        var tableNames = new (string Legacy, string Canonical)[]
        {
            ("sec_AspNetUsers", "sec_aspnetusers"),
            ("sec_AspNetRoles", "sec_aspnetroles"),
            ("sec_AspNetUserRoles", "sec_aspnetuserroles"),
            ("sec_AspNetUserClaims", "sec_aspnetuserclaims"),
            ("sec_AspNetUserLogins", "sec_aspnetuserlogins"),
            ("sec_AspNetRoleClaims", "sec_aspnetroleclaims"),
            ("sec_AspNetUserTokens", "sec_aspnetusertokens"),
            ("gnr_Brands", "gnr_brands"),
            ("gnr_Categories", "gnr_categories"),
            ("gnr_Commodities", "gnr_commodities"),
            ("gnr_Products", "gnr_products"),
            ("gnr_ProductVariants", "gnr_productvariants"),
            ("gnr_ProductTags", "gnr_producttags"),
            ("gnr_ProductProductTags", "gnr_productproducttags"),
            ("gnr_ProductSpecDefinitions", "gnr_productspecdefinitions"),
            ("gnr_ProductSpecValues", "gnr_productspecvalues"),
            ("cbi_Customers", "cbi_customers"),
            ("com_Orders", "com_orders"),
            ("com_OrderItems", "com_orderitems"),
            ("wf_PriceProposals", "wf_priceproposals"),
            ("wf_InventoryProposals", "wf_inventoryproposals"),
            ("wf_ApprovalAuditLogs", "wf_approvalauditlogs"),
            ("sync_OutboxMessages", "sync_outboxmessages"),
            ("sup_WooCommerceSyncLogs", "sup_woocommercesynclogs"),
            ("sup_GatewayPaymentReceipts", "sup_gatewaypaymentreceipts"),
            ("ord_WooCommerceOrderSnapshots", "ord_woocommerceordersnapshots"),
            ("ord_WooCommerceOrderItemSnapshots", "ord_woocommerceorderitemsnapshots"),
            ("pay_BarookPaymentSessions", "pay_barookpaymentsessions"),
            ("pay_ManualPaymentReceipts", "pay_manualpaymentreceipts")
        };

        foreach (var table in tableNames)
        {
            RenameTableIfNeeded(migrationBuilder, table.Legacy, table.Canonical);
        }
    }

    private static void CreateCanonicalTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `all_product_with_process` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ImportBatchId` VARCHAR(64) NULL,
  `SourceRowNumber` INT NULL,
  `SourceRowHash` CHAR(64) NOT NULL,
  `RawJson` LONGTEXT NULL,
  `MainProductName` VARCHAR(500) NULL,
  `MainProductSlug` VARCHAR(500) NULL,
  `GroupName` VARCHAR(500) NULL,
  `CategoryName` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `SKU` VARCHAR(191) NULL,
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageName` VARCHAR(300) NULL,
  `UnitWeight` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `CartonQuantity` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `SalePriceCash` DECIMAL(18,2) NULL,
  `SalePriceInstallment` DECIMAL(18,2) NULL,
  `PurchasePriceCash` DECIMAL(18,2) NULL,
  `PurchasePriceInstallment` DECIMAL(18,2) NULL,
  `ShortDescription` LONGTEXT NULL,
  `FullDescription` LONGTEXT NULL,
  `ImageUrl` LONGTEXT NULL,
  `GalleryJson` LONGTEXT NULL,
  `Status` VARCHAR(100) NULL,
  `WooProductId` BIGINT NULL,
  `HaveOtherPackage` TINYINT(1) NULL,
  `PackageOne` VARCHAR(300) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_All_Product_With_Process_SourceRowHash` (`SourceRowHash`),
  KEY `IX_All_Product_With_Process_ProductName` (`ProductName`(191)),
  KEY `IX_All_Product_With_Process_MainProductName` (`MainProductName`(191)),
  KEY `IX_All_Product_With_Process_SKU` (`SKU`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_main_groups` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `MainProductName` VARCHAR(500) NULL,
  `MainProductSlug` VARCHAR(500) NULL,
  `CategoryName` VARCHAR(500) NULL,
  `EnTaxonomic` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `Description` LONGTEXT NULL,
  `ImageUrl` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SourceKey` VARCHAR(500) NULL,
  `Name` VARCHAR(500) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_product_main_groups_slug` (`MainProductSlug`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_sale_products` (
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
  `SaleMode` VARCHAR(80) NULL,
  `PriceCalculationBasis` VARCHAR(80) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_sale_products_hash` (`SourceRowHash`),
  KEY `IX_khb_sale_products_woo` (`WooProductId`),
  KEY `IX_khb_sale_products_sku` (`SKU`),
  KEY `IX_khb_sale_products_name` (`ProductName`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_source_product` (
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
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_category_map` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `CategoryName` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `WooCategoryId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_commodity` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `CommodityName` VARCHAR(500) NULL,
  `CommoditySlug` VARCHAR(500) NULL,
  `WooCommodityId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_package_type` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `PackageGroup` VARCHAR(50) NULL,
  `PackageCode` VARCHAR(50) NULL,
  `PackageTitle` VARCHAR(300) NULL,
  `UnitWeightKg` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `WooPackageId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_final` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` CHAR(64) NOT NULL,
  `MainGroupId` BIGINT NULL,
  `CategorySourceKey` VARCHAR(500) NULL,
  `CommoditySourceKey` VARCHAR(500) NULL,
  `PackageSourceKey` VARCHAR(500) NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `WooProductId` BIGINT NULL,
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
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageTitle` VARCHAR(300) NULL,
  `BulkWeightKg` DECIMAL(18,6) NULL,
  `MinPurchaseKg` DECIMAL(18,6) NULL,
  `ImageTag` VARCHAR(300) NULL,
  `SaleMode` VARCHAR(80) NULL,
  `PriceCalculationBasis` VARCHAR(80) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Final_SKU` (`SKU`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_update_queue` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` CHAR(64) NOT NULL,
  `EntityType` VARCHAR(80) NOT NULL DEFAULT 'product',
  `QueueStatus` VARCHAR(50) NOT NULL DEFAULT 'pending',
  `ActionType` VARCHAR(50) NOT NULL DEFAULT 'upsert',
  `SKU` VARCHAR(191) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `WooProductId` BIGINT NULL,
  `WooPayloadJson` LONGTEXT NULL,
  `LastError` LONGTEXT NULL,
  `JobId` CHAR(36) NULL,
  `TryCount` INT NOT NULL DEFAULT 0,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Update_Queue_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Update_Queue_Status` (`QueueStatus`),
  KEY `IX_khb_product_update_queue_JobId` (`JobId`)
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
  KEY `IX_KHB_Product_Price_History_Product` (`ProductSourceKey`,`ProductType`,`PriceType`,`IsCurrent`),
  KEY `IX_KHB_Product_Price_History_SKU` (`SKU`),
  KEY `IX_KHB_Product_Price_History_Date` (`ValidFromUtc`,`ValidToUtc`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_change_log` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ProductId` BIGINT NOT NULL,
  `ChangeType` VARCHAR(100) NOT NULL,
  `Summary` VARCHAR(1000) NULL,
  `Payload` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_khb_product_change_log_ProductId` (`ProductId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceType` VARCHAR(64) NOT NULL,
  `ExternalId` VARCHAR(191) NULL,
  `Slug` VARCHAR(255) NULL,
  `Title` VARCHAR(512) NULL,
  `RawJson` LONGTEXT NOT NULL,
  `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SourceUrl` TEXT NULL,
  `Name` VARCHAR(512) NULL,
  `Status` VARCHAR(128) NULL,
  `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_workflow_jobs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `JobId` CHAR(36) NOT NULL,
  `Type` VARCHAR(50) NOT NULL,
  `Status` VARCHAR(50) NOT NULL DEFAULT 'pending',
  `CurrentStep` VARCHAR(160) NOT NULL DEFAULT 'Pending',
  `TotalItems` INT NOT NULL DEFAULT 0,
  `ProcessedItems` INT NOT NULL DEFAULT 0,
  `SuccessCount` INT NOT NULL DEFAULT 0,
  `ErrorCount` INT NOT NULL DEFAULT 0,
  `DraftCount` INT NOT NULL DEFAULT 0,
  `SkippedCount` INT NOT NULL DEFAULT 0,
  `PendingCount` INT NOT NULL DEFAULT 0,
  `ProgressPercent` INT NOT NULL DEFAULT 0,
  `Message` VARCHAR(2000) NULL,
  `CreatedBy` VARCHAR(256) NULL,
  `StartedAtUtc` DATETIME(6) NULL,
  `FinishedAtUtc` DATETIME(6) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_khb_workflow_jobs_JobId` (`JobId`),
  KEY `IX_khb_workflow_jobs_Type_CreatedAtUtc` (`Type`,`CreatedAtUtc`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_workflow_job_logs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `WorkflowJobId` BIGINT NOT NULL,
  `JobId` CHAR(36) NOT NULL,
  `StepName` VARCHAR(160) NOT NULL,
  `EntityType` VARCHAR(100) NULL,
  `EntityId` VARCHAR(191) NULL,
  `SKU` VARCHAR(191) NULL,
  `Status` VARCHAR(50) NOT NULL,
  `Message` VARCHAR(4000) NULL,
  `RequestUrl` VARCHAR(2000) NULL,
  `ResponseCode` INT NULL,
  `ResponseBodySummary` VARCHAR(4000) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_khb_workflow_job_logs_JobId_CreatedAtUtc` (`JobId`,`CreatedAtUtc`),
  KEY `IX_khb_workflow_job_logs_WorkflowJobId` (`WorkflowJobId`),
  CONSTRAINT `FK_khb_workflow_job_logs_khb_workflow_jobs_WorkflowJobId`
    FOREIGN KEY (`WorkflowJobId`) REFERENCES `khb_workflow_jobs` (`Id`) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_woocommerce_connection_profiles` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `ProfileName` VARCHAR(160) NOT NULL,
  `EnvironmentType` VARCHAR(20) NOT NULL,
  `BaseUrl` VARCHAR(1000) NOT NULL,
  `ConsumerKey` VARCHAR(255) NOT NULL,
  `ProtectedConsumerSecret` LONGTEXT NOT NULL,
  `ApiVersion` VARCHAR(40) NOT NULL DEFAULT 'wc/v3',
  `VerifySsl` TINYINT(1) NOT NULL DEFAULT 1,
  `TimeoutSeconds` INT NOT NULL DEFAULT 30,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 0,
  `LastTestedAtUtc` DATETIME(6) NULL,
  `LastTestSucceeded` TINYINT(1) NULL,
  `LastTestMessage` VARCHAR(2000) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_khb_woocommerce_connection_profiles_ProfileName` (`ProfileName`),
  KEY `IX_khb_woocommerce_connection_profiles_EnvironmentType_IsActive` (`EnvironmentType`,`IsActive`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    private static void AddCanonicalColumns(MigrationBuilder migrationBuilder)
    {
        AddColumnIfMissing(migrationBuilder, "khb_category_map", "CategoryName", "`CategoryName` VARCHAR(500) NULL");

        AddColumnIfMissing(migrationBuilder, "khb_commodity", "CommodityName", "`CommodityName` VARCHAR(500) NULL");

        AddColumnIfMissing(migrationBuilder, "khb_package_type", "PackageTitle", "`PackageTitle` VARCHAR(300) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_package_type", "PackagingPricePerPack", "`PackagingPricePerPack` DECIMAL(18,2) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_package_type", "WooPackageId", "`WooPackageId` BIGINT NULL");

        AddColumnIfMissing(migrationBuilder, "khb_source_product", "ProductName", "`ProductName` VARCHAR(700) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "ProductEnglishName", "`ProductEnglishName` VARCHAR(700) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "MainProductName", "`MainProductName` VARCHAR(500) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "CategoryName", "`CategoryName` VARCHAR(500) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "BrandName", "`BrandName` VARCHAR(300) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "BrandEnglishName", "`BrandEnglishName` VARCHAR(300) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "UnitWeightKg", "`UnitWeightKg` DECIMAL(18,6) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "KgCashPrice", "`KgCashPrice` DECIMAL(18,2) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_source_product", "KgCreditPrice", "`KgCreditPrice` DECIMAL(18,2) NULL");

        var productFinalColumns = new (string Name, string Definition)[]
        {
            ("MainGroupId", "`MainGroupId` BIGINT NULL"),
            ("CategorySourceKey", "`CategorySourceKey` VARCHAR(500) NULL"),
            ("CommoditySourceKey", "`CommoditySourceKey` VARCHAR(500) NULL"),
            ("PackageSourceKey", "`PackageSourceKey` VARCHAR(500) NULL"),
            ("ProductName", "`ProductName` VARCHAR(700) NULL"),
            ("ProductEnglishName", "`ProductEnglishName` VARCHAR(700) NULL"),
            ("BrandName", "`BrandName` VARCHAR(300) NULL"),
            ("BrandEnglishName", "`BrandEnglishName` VARCHAR(300) NULL"),
            ("PackageTitle", "`PackageTitle` VARCHAR(300) NULL"),
            ("PackagingPricePerPack", "`PackagingPricePerPack` DECIMAL(18,2) NULL"),
            ("KgCashPrice", "`KgCashPrice` DECIMAL(18,2) NULL"),
            ("KgCreditPrice", "`KgCreditPrice` DECIMAL(18,2) NULL"),
            ("SaleCashPrice", "`SaleCashPrice` DECIMAL(18,2) NULL"),
            ("SaleCreditPrice", "`SaleCreditPrice` DECIMAL(18,2) NULL"),
            ("BuyCashPrice", "`BuyCashPrice` DECIMAL(18,2) NULL"),
            ("BuyCreditPrice", "`BuyCreditPrice` DECIMAL(18,2) NULL"),
            ("CatalogVisibility", "`CatalogVisibility` VARCHAR(50) NULL"),
            ("WooPayloadJson", "`WooPayloadJson` LONGTEXT NULL"),
            ("BulkWeightKg", "`BulkWeightKg` DECIMAL(18,6) NULL"),
            ("MinPurchaseKg", "`MinPurchaseKg` DECIMAL(18,6) NULL"),
            ("ImageTag", "`ImageTag` VARCHAR(300) NULL"),
            ("SaleMode", "`SaleMode` VARCHAR(80) NULL"),
            ("PriceCalculationBasis", "`PriceCalculationBasis` VARCHAR(80) NULL")
        };
        foreach (var column in productFinalColumns)
        {
            AddColumnIfMissing(migrationBuilder, "khb_product_final", column.Name, column.Definition);
        }

        var queueColumns = new (string Name, string Definition)[]
        {
            ("QueueStatus", "`QueueStatus` VARCHAR(50) NOT NULL DEFAULT 'pending'"),
            ("ActionType", "`ActionType` VARCHAR(50) NOT NULL DEFAULT 'upsert'"),
            ("SKU", "`SKU` VARCHAR(191) NULL"),
            ("ProductSlug", "`ProductSlug` VARCHAR(700) NULL"),
            ("WooProductId", "`WooProductId` BIGINT NULL"),
            ("WooPayloadJson", "`WooPayloadJson` LONGTEXT NULL"),
            ("JobId", "`JobId` CHAR(36) NULL"),
            ("TryCount", "`TryCount` INT NOT NULL DEFAULT 0")
        };
        foreach (var column in queueColumns)
        {
            AddColumnIfMissing(migrationBuilder, "khb_product_update_queue", column.Name, column.Definition);
        }

        AddColumnIfMissing(migrationBuilder, "khb_imported_woocommerce_records", "Slug", "`Slug` VARCHAR(255) NULL");
        AddColumnIfMissing(migrationBuilder, "khb_imported_woocommerce_records", "Title", "`Title` VARCHAR(512) NULL");
    }

    private static void BackfillCanonicalColumns(MigrationBuilder migrationBuilder)
    {
        BackfillIfBothColumnsExist(migrationBuilder, "khb_category_map", "CategoryName", "CategoryNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_commodity", "CommodityName", "CommodityNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_package_type", "PackageTitle", "PackageNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_package_type", "PackagingPricePerPack", "PackagingPricePerPackToman");

        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "ProductName", "ProductNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "ProductEnglishName", "ProductNameEn");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "MainProductName", "MainProductNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "CategoryName", "CategoryNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "BrandName", "BrandNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "BrandEnglishName", "BrandNameEn");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "UnitWeightKg", "PackageOneKg");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "KgCashPrice", "KgCashPriceToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_source_product", "KgCreditPrice", "KgCreditPriceToman");

        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "ProductName", "ProductNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "ProductEnglishName", "ProductNameEn");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "BrandName", "BrandNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "BrandEnglishName", "BrandNameEn");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "PackageTitle", "PackageNameFa");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "PackagingPricePerPack", "PackagingPricePerPackToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "KgCashPrice", "KgCashPriceToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "KgCreditPrice", "KgCreditPriceToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "SaleCashPrice", "SalePriceCashToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "SaleCreditPrice", "SalePriceCreditToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "BuyCashPrice", "PurchasePriceCashToman");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_final", "BuyCreditPrice", "PurchasePriceCreditToman");

        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_update_queue", "QueueStatus", "SyncStatus");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_update_queue", "WooPayloadJson", "PayloadJson");
        BackfillIfBothColumnsExist(migrationBuilder, "khb_product_update_queue", "WooProductId", "WooObjectId");
    }

    private static void AddCanonicalIndexes(MigrationBuilder migrationBuilder)
    {
        AddIndexIfMissing(migrationBuilder, "khb_package_type", "IX_khb_package_type_WooPackageId", "`WooPackageId`");
        AddIndexIfMissing(migrationBuilder, "khb_product_final", "IX_khb_product_final_ProductSlug", "`ProductSlug`(191)");
        AddIndexIfMissing(migrationBuilder, "khb_product_final", "IX_khb_product_final_WooProductId", "`WooProductId`");
        AddIndexIfMissing(migrationBuilder, "khb_product_update_queue", "IX_khb_product_update_queue_JobId", "`JobId`");
        AddUniqueIndexIfClean(
            migrationBuilder,
            "khb_imported_woocommerce_records",
            "UX_khb_imported_woocommerce_records_Source_External",
            "`SourceType`, `ExternalId`",
            "`ExternalId` IS NOT NULL",
            "`SourceType`, `ExternalId`");
    }

    private static void RenameTableIfNeeded(MigrationBuilder migrationBuilder, string legacyName, string canonicalName)
    {
        migrationBuilder.Sql($@"
SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(legacyName)}'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(canonicalName)}'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `{EscapeIdentifier(legacyName)}` TO `{EscapeIdentifier(canonicalName)}`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }

    private static void AddColumnIfMissing(MigrationBuilder migrationBuilder, string tableName, string columnName, string definition)
    {
        migrationBuilder.Sql($@"
SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY '{Escape(tableName)}'
    AND BINARY COLUMN_NAME = BINARY '{Escape(columnName)}'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `{EscapeIdentifier(tableName)}` ADD COLUMN {EscapeSqlLiteral(definition)}',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }

    private static void BackfillIfBothColumnsExist(MigrationBuilder migrationBuilder, string tableName, string targetColumn, string sourceColumn)
    {
        migrationBuilder.Sql($@"
SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(tableName)}'
    AND BINARY COLUMN_NAME = BINARY '{Escape(targetColumn)}'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(tableName)}'
    AND BINARY COLUMN_NAME = BINARY '{Escape(sourceColumn)}'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `{EscapeIdentifier(tableName)}` SET `{EscapeIdentifier(targetColumn)}` = `{EscapeIdentifier(sourceColumn)}` WHERE `{EscapeIdentifier(targetColumn)}` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }

    private static void AddIndexIfMissing(MigrationBuilder migrationBuilder, string tableName, string indexName, string columns)
    {
        migrationBuilder.Sql($@"
SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(tableName)}'
    AND BINARY INDEX_NAME = BINARY '{Escape(indexName)}'
);
SET @khb_sql = IF(
  @khb_index_exists = 0,
  'ALTER TABLE `{EscapeIdentifier(tableName)}` ADD INDEX `{EscapeIdentifier(indexName)}` ({EscapeSqlLiteral(columns)})',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }

    private static void AddUniqueIndexIfClean(
        MigrationBuilder migrationBuilder,
        string tableName,
        string indexName,
        string columns,
        string where,
        string groupBy)
    {
        migrationBuilder.Sql($@"
SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY '{Escape(tableName)}'
    AND BINARY INDEX_NAME = BINARY '{Escape(indexName)}'
);
SET @khb_duplicate_count = (
  SELECT COUNT(*) FROM (
    SELECT 1 FROM `{EscapeIdentifier(tableName)}`
    WHERE {where}
    GROUP BY {groupBy}
    HAVING COUNT(*) > 1
  ) khb_duplicates
);
SET @khb_sql = IF(
  @khb_index_exists = 0 AND @khb_duplicate_count = 0,
  'ALTER TABLE `{EscapeIdentifier(tableName)}` ADD UNIQUE INDEX `{EscapeIdentifier(indexName)}` ({EscapeSqlLiteral(columns)})',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }

    private static string Escape(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    private static string EscapeIdentifier(string value) => value.Replace("`", "``", StringComparison.Ordinal);
    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
