CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `cbi_Customers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FullName` varchar(250) NOT NULL,
        `PhoneNumber` varchar(30) NOT NULL,
        `Email` varchar(320) NULL,
        `AddressLine` varchar(1000) NOT NULL,
        `City` varchar(150) NOT NULL,
        `PostalCode` varchar(30) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_Brands` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(160) NOT NULL,
        `Slug` varchar(180) NOT NULL,
        `LogoUrl` varchar(1000) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `WooCommerceBrandId` bigint NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_Categories` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(150) NOT NULL,
        `Slug` varchar(180) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_Commodities` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(180) NOT NULL,
        `Slug` varchar(200) NOT NULL,
        `EnglishName` varchar(120) NULL,
        `Description` varchar(1000) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_ProductSpecDefinitions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(140) NOT NULL,
        `Slug` varchar(160) NOT NULL,
        `Unit` varchar(40) NULL,
        `SortOrder` int NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_ProductTags` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(120) NOT NULL,
        `Slug` varchar(140) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `ord_WooCommerceOrderSnapshots` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `WooCommerceOrderId` bigint NOT NULL,
        `WooCommerceOrderNumber` varchar(80) NOT NULL,
        `WooCommerceStatus` varchar(80) NOT NULL,
        `InternalStatus` varchar(80) NOT NULL,
        `PaymentStatus` varchar(80) NOT NULL,
        `PaymentMethod` varchar(120) NULL,
        `PaymentMethodTitle` varchar(200) NULL,
        `TransactionId` varchar(160) NULL,
        `Currency` varchar(8) NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `ShippingTotal` decimal(18,2) NOT NULL,
        `DiscountTotal` decimal(18,2) NOT NULL,
        `CustomerFullName` varchar(250) NOT NULL,
        `CustomerPhone` varchar(40) NULL,
        `CustomerEmail` varchar(320) NULL,
        `CustomerNationalCode` varchar(20) NULL,
        `BillingAddress` varchar(1500) NULL,
        `ShippingAddress` varchar(1500) NULL,
        `CustomerNote` varchar(2000) NULL,
        `WooCreatedAtUtc` datetime(6) NULL,
        `WooUpdatedAtUtc` datetime(6) NULL,
        `SyncedAtUtc` datetime(6) NOT NULL,
        `LastPaymentCheckedAtUtc` datetime(6) NULL,
        `ReadyToShipAtUtc` datetime(6) NULL,
        `LastActionByUserName` varchar(256) NULL,
        `LastActionNote` varchar(1000) NULL,
        `RawJson` longtext NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetRoles` (
        `Id` varchar(255) NOT NULL,
        `Name` varchar(256) NULL,
        `NormalizedName` varchar(256) NULL,
        `ConcurrencyStamp` longtext NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetUsers` (
        `Id` varchar(255) NOT NULL,
        `FullName` longtext NULL,
        `UserName` varchar(256) NULL,
        `NormalizedUserName` varchar(256) NULL,
        `Email` varchar(256) NULL,
        `NormalizedEmail` varchar(256) NULL,
        `EmailConfirmed` tinyint(1) NOT NULL,
        `PasswordHash` longtext NULL,
        `SecurityStamp` longtext NULL,
        `ConcurrencyStamp` longtext NULL,
        `PhoneNumber` longtext NULL,
        `PhoneNumberConfirmed` tinyint(1) NOT NULL,
        `TwoFactorEnabled` tinyint(1) NOT NULL,
        `LockoutEnd` datetime NULL,
        `LockoutEnabled` tinyint(1) NOT NULL,
        `AccessFailedCount` int NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sup_GatewayPaymentReceipts` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `PaidAtUtc` datetime(6) NULL,
        `WooCommerceOrderId` bigint NOT NULL,
        `LocalOrderId` int NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Currency` varchar(8) NOT NULL,
        `GatewayName` varchar(80) NOT NULL,
        `TransactionId` varchar(160) NOT NULL,
        `IdempotencyKey` varchar(200) NOT NULL,
        `PaymentStatus` varchar(50) NOT NULL,
        `GatewayRawStatus` varchar(100) NULL,
        `Note` varchar(1000) NULL,
        `RequestedByUserName` varchar(256) NOT NULL,
        `SentToWooCommerce` tinyint(1) NOT NULL,
        `SentToWooCommerceAtUtc` datetime(6) NULL,
        `WooCommerceResponseSummary` varchar(2000) NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sup_WooCommerceSyncLogs` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `Operation` varchar(80) NOT NULL,
        `Status` varchar(40) NOT NULL,
        `EntityType` varchar(80) NULL,
        `LocalEntityId` int NULL,
        `WooCommerceEntityId` bigint NULL,
        `RequestHash` varchar(128) NULL,
        `Message` varchar(2000) NULL,
        `PerformedByUserName` varchar(256) NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sync_OutboxMessages` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `EventType` varchar(120) NOT NULL,
        `AggregateType` varchar(120) NOT NULL,
        `AggregateId` bigint NOT NULL,
        `PayloadJson` longtext NOT NULL,
        `Status` int NOT NULL,
        `QueuedByUserName` varchar(256) NOT NULL,
        `SourceWorkflow` varchar(120) NOT NULL,
        `LastError` varchar(1000) NULL,
        `LockedBy` varchar(256) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `LockedAtUtc` datetime(6) NULL,
        `SentAtUtc` datetime(6) NULL,
        `RetryCount` int NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `wf_ApprovalAuditLogs` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `EntityType` varchar(80) NOT NULL,
        `EntityId` bigint NOT NULL,
        `Action` varchar(80) NOT NULL,
        `UserName` varchar(256) NOT NULL,
        `UserRole` varchar(80) NOT NULL,
        `Note` varchar(1000) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `com_Orders` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `CreatedAt` datetime(6) NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `Status` varchar(50) NOT NULL,
        `PaymentMethod` varchar(50) NOT NULL,
        `PaymentStatus` varchar(50) NOT NULL,
        `PaymentReference` varchar(160) NULL,
        `GatewayName` varchar(80) NULL,
        `PaidAtUtc` datetime(6) NULL,
        `WooCommerceOrderId` bigint NULL,
        `CustomerId` int NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_com_Orders_cbi_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `cbi_Customers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_Products` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(250) NOT NULL,
        `Slug` varchar(280) NOT NULL,
        `Description` varchar(4000) NOT NULL,
        `IsAvailable` tinyint(1) NOT NULL,
        `CategoryId` int NOT NULL,
        `BrandId` int NULL,
        `CommodityId` int NULL,
        `ImageUrl` varchar(1000) NULL,
        `GalleryJson` longtext NULL,
        `Sku` varchar(120) NULL,
        `Price` decimal(18,2) NOT NULL,
        `OldPrice` decimal(18,2) NULL,
        `PurchasePrice` decimal(18,2) NULL,
        `StockQuantity` int NOT NULL,
        `MinStockAlertQuantity` int NULL,
        `WooCommerceProductId` bigint NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `UpdatedAtUtc` datetime(6) NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_gnr_Products_gnr_Brands_BrandId` FOREIGN KEY (`BrandId`) REFERENCES `gnr_Brands` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_gnr_Products_gnr_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `gnr_Categories` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_gnr_Products_gnr_Commodities_CommodityId` FOREIGN KEY (`CommodityId`) REFERENCES `gnr_Commodities` (`Id`) ON DELETE SET NULL
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `ord_WooCommerceOrderItemSnapshots` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `WooCommerceOrderSnapshotId` bigint NOT NULL,
        `WooCommerceLineItemId` bigint NOT NULL,
        `WooCommerceProductId` bigint NULL,
        `WooCommerceVariationId` bigint NULL,
        `Sku` varchar(120) NULL,
        `Name` varchar(300) NOT NULL,
        `UnitType` varchar(50) NOT NULL,
        `Quantity` decimal(18,3) NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `LineTotal` decimal(18,2) NOT NULL,
        `RawJson` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ord_WooCommerceOrderItemSnapshots_ord_WooCommerceOrderSnapsh~` FOREIGN KEY (`WooCommerceOrderSnapshotId`) REFERENCES `ord_WooCommerceOrderSnapshots` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `pay_BarookPaymentSessions` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `WooCommerceOrderSnapshotId` bigint NOT NULL,
        `WooCommerceOrderId` bigint NOT NULL,
        `ExternalCode` varchar(120) NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Currency` varchar(8) NOT NULL,
        `Token` varchar(200) NULL,
        `TokenExpireDateUtc` datetime(6) NULL,
        `PaymentUrl` varchar(1000) NULL,
        `LinkSentAtUtc` datetime(6) NULL,
        `BarookStatus` varchar(80) NULL,
        `ReferenceNumber` varchar(160) NULL,
        `MaskedCardNumber` varchar(40) NULL,
        `TransactionId` varchar(160) NULL,
        `PaidAtUtc` datetime(6) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `VerifiedAtUtc` datetime(6) NULL,
        `CreatedByUserName` varchar(256) NOT NULL,
        `VerifiedByUserName` varchar(256) NULL,
        `StartRequestJson` longtext NULL,
        `StartResponseJson` longtext NULL,
        `VerifyResponseJson` longtext NULL,
        `LastError` varchar(2000) NULL,
        `IsCompleted` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_pay_BarookPaymentSessions_ord_WooCommerceOrderSnapshots_WooC~` FOREIGN KEY (`WooCommerceOrderSnapshotId`) REFERENCES `ord_WooCommerceOrderSnapshots` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `pay_ManualPaymentReceipts` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `WooCommerceOrderSnapshotId` bigint NOT NULL,
        `WooCommerceOrderId` bigint NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Currency` varchar(8) NOT NULL,
        `ReceiptNumber` varchar(160) NOT NULL,
        `PaymentSource` varchar(80) NOT NULL,
        `PaidAtUtc` datetime(6) NOT NULL,
        `RegisteredByUserName` varchar(256) NOT NULL,
        `Note` varchar(1000) NULL,
        `SentToWooCommerce` tinyint(1) NOT NULL,
        `SentToWooCommerceAtUtc` datetime(6) NULL,
        `WooCommerceResponseSummary` varchar(2000) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_pay_ManualPaymentReceipts_ord_WooCommerceOrderSnapshots_WooC~` FOREIGN KEY (`WooCommerceOrderSnapshotId`) REFERENCES `ord_WooCommerceOrderSnapshots` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetRoleClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `RoleId` varchar(255) NOT NULL,
        `ClaimType` longtext NULL,
        `ClaimValue` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_sec_AspNetRoleClaims_sec_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `sec_AspNetRoles` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetUserClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` varchar(255) NOT NULL,
        `ClaimType` longtext NULL,
        `ClaimValue` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_sec_AspNetUserClaims_sec_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `sec_AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetUserLogins` (
        `LoginProvider` varchar(255) NOT NULL,
        `ProviderKey` varchar(255) NOT NULL,
        `ProviderDisplayName` longtext NULL,
        `UserId` varchar(255) NOT NULL,
        PRIMARY KEY (`LoginProvider`, `ProviderKey`),
        CONSTRAINT `FK_sec_AspNetUserLogins_sec_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `sec_AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetUserRoles` (
        `UserId` varchar(255) NOT NULL,
        `RoleId` varchar(255) NOT NULL,
        PRIMARY KEY (`UserId`, `RoleId`),
        CONSTRAINT `FK_sec_AspNetUserRoles_sec_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `sec_AspNetRoles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_sec_AspNetUserRoles_sec_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `sec_AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `sec_AspNetUserTokens` (
        `UserId` varchar(255) NOT NULL,
        `LoginProvider` varchar(255) NOT NULL,
        `Name` varchar(255) NOT NULL,
        `Value` longtext NULL,
        PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
        CONSTRAINT `FK_sec_AspNetUserTokens_sec_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `sec_AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_ProductProductTags` (
        `ProductId` int NOT NULL,
        `ProductTagId` int NOT NULL,
        PRIMARY KEY (`ProductId`, `ProductTagId`),
        CONSTRAINT `FK_gnr_ProductProductTags_gnr_ProductTags_ProductTagId` FOREIGN KEY (`ProductTagId`) REFERENCES `gnr_ProductTags` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_gnr_ProductProductTags_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_ProductSpecValues` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ProductId` int NOT NULL,
        `SpecDefinitionId` int NOT NULL,
        `Value` varchar(500) NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_gnr_ProductSpecValues_gnr_ProductSpecDefinitions_SpecDefinit~` FOREIGN KEY (`SpecDefinitionId`) REFERENCES `gnr_ProductSpecDefinitions` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_gnr_ProductSpecValues_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `gnr_ProductVariants` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(150) NOT NULL,
        `Sku` varchar(120) NULL,
        `Price` decimal(18,2) NOT NULL,
        `OldPrice` decimal(18,2) NULL,
        `PurchasePrice` decimal(18,2) NULL,
        `StockQuantity` int NOT NULL,
        `MinStockAlertQuantity` int NULL,
        `IsDefault` tinyint(1) NOT NULL,
        `IsAvailable` tinyint(1) NOT NULL,
        `WooCommerceProductId` bigint NULL,
        `WooCommerceVariationId` bigint NULL,
        `ProductId` int NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_gnr_ProductVariants_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `com_OrderItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `OrderId` int NOT NULL,
        `ProductId` int NOT NULL,
        `ProductVariantId` int NULL,
        `VariantName` varchar(150) NULL,
        `Sku` varchar(120) NULL,
        `WooCommerceProductId` bigint NULL,
        `WooCommerceVariationId` bigint NULL,
        `Quantity` int NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_com_OrderItems_com_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `com_Orders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_com_OrderItems_gnr_ProductVariants_ProductVariantId` FOREIGN KEY (`ProductVariantId`) REFERENCES `gnr_ProductVariants` (`Id`),
        CONSTRAINT `FK_com_OrderItems_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `wf_InventoryProposals` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `ProductId` int NOT NULL,
        `ProductVariantId` int NULL,
        `CurrentStockQuantity` int NOT NULL,
        `ProposedQuantity` int NOT NULL,
        `FinalStockQuantity` int NOT NULL,
        `AdjustmentKind` int NOT NULL,
        `Status` int NOT NULL,
        `CreatedByUserName` varchar(256) NOT NULL,
        `ManagerApprovedByUserName` varchar(256) NULL,
        `SuperAdminApprovedByUserName` varchar(256) NULL,
        `RejectedByUserName` varchar(256) NULL,
        `Reason` varchar(1000) NULL,
        `ManagerNote` varchar(1000) NULL,
        `SuperAdminNote` varchar(1000) NULL,
        `RejectionReason` varchar(1000) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `ManagerApprovedAtUtc` datetime(6) NULL,
        `SuperAdminApprovedAtUtc` datetime(6) NULL,
        `RejectedAtUtc` datetime(6) NULL,
        `QueuedForSyncAtUtc` datetime(6) NULL,
        `SyncedAtUtc` datetime(6) NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_wf_InventoryProposals_gnr_ProductVariants_ProductVariantId` FOREIGN KEY (`ProductVariantId`) REFERENCES `gnr_ProductVariants` (`Id`),
        CONSTRAINT `FK_wf_InventoryProposals_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE TABLE `wf_PriceProposals` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `ProductId` int NOT NULL,
        `ProductVariantId` int NULL,
        `CurrentSalePrice` decimal(18,2) NOT NULL,
        `ProposedSalePrice` decimal(18,2) NOT NULL,
        `CurrentPurchasePrice` decimal(18,2) NULL,
        `ProposedPurchasePrice` decimal(18,2) NULL,
        `Status` int NOT NULL,
        `CreatedByUserName` varchar(256) NOT NULL,
        `ManagerApprovedByUserName` varchar(256) NULL,
        `SuperAdminApprovedByUserName` varchar(256) NULL,
        `RejectedByUserName` varchar(256) NULL,
        `Reason` varchar(1000) NULL,
        `ManagerNote` varchar(1000) NULL,
        `SuperAdminNote` varchar(1000) NULL,
        `RejectionReason` varchar(1000) NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `ManagerApprovedAtUtc` datetime(6) NULL,
        `SuperAdminApprovedAtUtc` datetime(6) NULL,
        `RejectedAtUtc` datetime(6) NULL,
        `QueuedForSyncAtUtc` datetime(6) NULL,
        `SyncedAtUtc` datetime(6) NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_wf_PriceProposals_gnr_ProductVariants_ProductVariantId` FOREIGN KEY (`ProductVariantId`) REFERENCES `gnr_ProductVariants` (`Id`),
        CONSTRAINT `FK_wf_PriceProposals_gnr_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `gnr_Products` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    INSERT INTO `gnr_Categories` (`Id`, `Name`, `Slug`)
    VALUES (1, 'لوبیا', 'beans');
    SELECT ROW_COUNT();

    INSERT INTO `gnr_Categories` (`Id`, `Name`, `Slug`)
    VALUES (2, 'عدس', 'lentils');
    SELECT ROW_COUNT();

    INSERT INTO `gnr_Categories` (`Id`, `Name`, `Slug`)
    VALUES (3, 'نخود', 'chickpeas');
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('0c5e0418-46b3-4c6e-887e-0c182171ab11', 'd8b522b7-bf1b-4acb-ab32-dac6ee529d90', 'SalesManager', 'SALESMANAGER');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('4f43b487-3f8e-426d-9a46-048c7d07f7f9', '4d2df8dd-8f24-4fb6-ac1f-4db646cfb66f', 'SuperAdmin', 'SUPERADMIN');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('5f36c2f9-330a-492c-8ebf-65141782f2bb', 'd4a7dab0-72ec-4f45-ac7f-8dfcd9158b67', 'PricingEmployee', 'PRICINGEMPLOYEE');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('6240e185-5c3a-410b-99d3-9767571fdf24', '7d9e2b52-7518-49c7-ad91-0625deb5448a', 'Customer', 'CUSTOMER');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('67320cb2-92a2-4de7-971b-7e9e80244f4b', 'fa971a2e-ce68-4314-87ca-92ad7835d7c4', 'GatewayAdmin', 'GATEWAYADMIN');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c', 'e7a60e90-aae7-4a36-b60b-0307df4a3418', 'CentralSyncAgent', 'CENTRALSYNCAGENT');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('b1477f6c-54ef-48d0-b24c-756b3a83b1a1', '7c01a114-3ede-469f-83b3-9fe7e457d82a', 'PricingManager', 'PRICINGMANAGER');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('e572b070-82bd-47f0-b486-cc1b644b2d9e', '5a55800b-8979-479f-b45e-896e7f852248', 'ShippingOrderManager', 'SHIPPINGORDERMANAGER');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('e5ac8272-7f9f-47c0-8e21-040fe3d242ed', 'a9735c29-6424-4ed0-af63-fdba24f66130', 'WarehouseEmployee', 'WAREHOUSEEMPLOYEE');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52', '10c20816-f415-4315-bc76-89af7265626f', 'Accountant', 'ACCOUNTANT');
    SELECT ROW_COUNT();

    INSERT INTO `sec_AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
    VALUES ('f517b79d-1fc4-4800-bcb8-ee0ca67dce1e', '50819a95-8ece-4c30-9adb-4e4292ae1302', 'Admin', 'ADMIN');
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_OrderItems_OrderId` ON `com_OrderItems` (`OrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_OrderItems_ProductId` ON `com_OrderItems` (`ProductId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_OrderItems_ProductVariantId` ON `com_OrderItems` (`ProductVariantId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_Orders_CreatedAt` ON `com_Orders` (`CreatedAt`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_Orders_CustomerId` ON `com_Orders` (`CustomerId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_com_Orders_WooCommerceOrderId` ON `com_Orders` (`WooCommerceOrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Brands_Slug` ON `gnr_Brands` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Brands_WooCommerceBrandId` ON `gnr_Brands` (`WooCommerceBrandId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Categories_Slug` ON `gnr_Categories` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Commodities_Slug` ON `gnr_Commodities` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_gnr_ProductProductTags_ProductTagId` ON `gnr_ProductProductTags` (`ProductTagId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_gnr_Products_BrandId` ON `gnr_Products` (`BrandId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_gnr_Products_CategoryId` ON `gnr_Products` (`CategoryId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_gnr_Products_CommodityId` ON `gnr_Products` (`CommodityId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Products_Sku` ON `gnr_Products` (`Sku`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Products_Slug` ON `gnr_Products` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_Products_WooCommerceProductId` ON `gnr_Products` (`WooCommerceProductId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductSpecDefinitions_Slug` ON `gnr_ProductSpecDefinitions` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductSpecValues_ProductId_SpecDefinitionId` ON `gnr_ProductSpecValues` (`ProductId`, `SpecDefinitionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_gnr_ProductSpecValues_SpecDefinitionId` ON `gnr_ProductSpecValues` (`SpecDefinitionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductTags_Slug` ON `gnr_ProductTags` (`Slug`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductVariants_ProductId_Name` ON `gnr_ProductVariants` (`ProductId`, `Name`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductVariants_Sku` ON `gnr_ProductVariants` (`Sku`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_gnr_ProductVariants_WooCommerceProductId_WooCommerceVariatio~` ON `gnr_ProductVariants` (`WooCommerceProductId`, `WooCommerceVariationId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_ord_WooCommerceOrderItemSnapshots_WooCommerceOrderSnapshotId~` ON `ord_WooCommerceOrderItemSnapshots` (`WooCommerceOrderSnapshotId`, `WooCommerceLineItemId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderItemSnapshots_WooCommerceProductId` ON `ord_WooCommerceOrderItemSnapshots` (`WooCommerceProductId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderItemSnapshots_WooCommerceVariationId` ON `ord_WooCommerceOrderItemSnapshots` (`WooCommerceVariationId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_CustomerNationalCode` ON `ord_WooCommerceOrderSnapshots` (`CustomerNationalCode`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_CustomerPhone` ON `ord_WooCommerceOrderSnapshots` (`CustomerPhone`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_InternalStatus` ON `ord_WooCommerceOrderSnapshots` (`InternalStatus`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_PaymentStatus` ON `ord_WooCommerceOrderSnapshots` (`PaymentStatus`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderId` ON `ord_WooCommerceOrderSnapshots` (`WooCommerceOrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderNumber` ON `ord_WooCommerceOrderSnapshots` (`WooCommerceOrderNumber`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_WooCommerceStatus` ON `ord_WooCommerceOrderSnapshots` (`WooCommerceStatus`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_ord_WooCommerceOrderSnapshots_WooCreatedAtUtc` ON `ord_WooCommerceOrderSnapshots` (`WooCreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_BarookPaymentSessions_BarookStatus` ON `pay_BarookPaymentSessions` (`BarookStatus`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_pay_BarookPaymentSessions_ExternalCode` ON `pay_BarookPaymentSessions` (`ExternalCode`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_BarookPaymentSessions_Token` ON `pay_BarookPaymentSessions` (`Token`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_BarookPaymentSessions_WooCommerceOrderId` ON `pay_BarookPaymentSessions` (`WooCommerceOrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_BarookPaymentSessions_WooCommerceOrderSnapshotId` ON `pay_BarookPaymentSessions` (`WooCommerceOrderSnapshotId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_pay_ManualPaymentReceipts_ReceiptNumber` ON `pay_ManualPaymentReceipts` (`ReceiptNumber`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_ManualPaymentReceipts_WooCommerceOrderId` ON `pay_ManualPaymentReceipts` (`WooCommerceOrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_pay_ManualPaymentReceipts_WooCommerceOrderSnapshotId` ON `pay_ManualPaymentReceipts` (`WooCommerceOrderSnapshotId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sec_AspNetRoleClaims_RoleId` ON `sec_AspNetRoleClaims` (`RoleId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `RoleNameIndex` ON `sec_AspNetRoles` (`NormalizedName`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sec_AspNetUserClaims_UserId` ON `sec_AspNetUserClaims` (`UserId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sec_AspNetUserLogins_UserId` ON `sec_AspNetUserLogins` (`UserId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sec_AspNetUserRoles_RoleId` ON `sec_AspNetUserRoles` (`RoleId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `EmailIndex` ON `sec_AspNetUsers` (`NormalizedEmail`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `UserNameIndex` ON `sec_AspNetUsers` (`NormalizedUserName`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_sup_GatewayPaymentReceipts_IdempotencyKey` ON `sup_GatewayPaymentReceipts` (`IdempotencyKey`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE UNIQUE INDEX `IX_sup_GatewayPaymentReceipts_TransactionId` ON `sup_GatewayPaymentReceipts` (`TransactionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sup_GatewayPaymentReceipts_WooCommerceOrderId` ON `sup_GatewayPaymentReceipts` (`WooCommerceOrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sup_WooCommerceSyncLogs_CreatedAtUtc` ON `sup_WooCommerceSyncLogs` (`CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sup_WooCommerceSyncLogs_Operation_Status` ON `sup_WooCommerceSyncLogs` (`Operation`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sync_OutboxMessages_AggregateType_AggregateId` ON `sync_OutboxMessages` (`AggregateType`, `AggregateId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sync_OutboxMessages_CreatedAtUtc` ON `sync_OutboxMessages` (`CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_sync_OutboxMessages_Status` ON `sync_OutboxMessages` (`Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_ApprovalAuditLogs_CreatedAtUtc` ON `wf_ApprovalAuditLogs` (`CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_ApprovalAuditLogs_EntityType_EntityId` ON `wf_ApprovalAuditLogs` (`EntityType`, `EntityId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_InventoryProposals_CreatedAtUtc` ON `wf_InventoryProposals` (`CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_InventoryProposals_ProductId` ON `wf_InventoryProposals` (`ProductId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_InventoryProposals_ProductVariantId` ON `wf_InventoryProposals` (`ProductVariantId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_InventoryProposals_Status` ON `wf_InventoryProposals` (`Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_PriceProposals_CreatedAtUtc` ON `wf_PriceProposals` (`CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_PriceProposals_ProductId` ON `wf_PriceProposals` (`ProductId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_PriceProposals_ProductVariantId` ON `wf_PriceProposals` (`ProductVariantId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    CREATE INDEX `IX_wf_PriceProposals_Status` ON `wf_PriceProposals` (`Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623172007_InitialMySqlKharbarchi')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260623172007_InitialMySqlKharbarchi', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '3a3f068b-ae89-4dcd-9020-3dfcb889d814'
    WHERE `Id` = '0c5e0418-46b3-4c6e-887e-0c182171ab11';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'be2ca41b-2f03-4ba5-97ef-faf79467ef3c'
    WHERE `Id` = '4f43b487-3f8e-426d-9a46-048c7d07f7f9';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '1da09220-d3b7-473b-8e04-9922b6806f34'
    WHERE `Id` = '5f36c2f9-330a-492c-8ebf-65141782f2bb';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '876fdd6e-b88f-4967-a6c8-7005462ca8ee'
    WHERE `Id` = '6240e185-5c3a-410b-99d3-9767571fdf24';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'ca0e92e4-4676-4c47-ba32-c916a32b3e20'
    WHERE `Id` = '67320cb2-92a2-4de7-971b-7e9e80244f4b';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '9c6142f8-6ed7-4502-aeb0-6f44b0987a17'
    WHERE `Id` = '9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'b102ddc1-b72c-40be-99ec-cce6d718544b'
    WHERE `Id` = 'b1477f6c-54ef-48d0-b24c-756b3a83b1a1';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '231e6f04-589f-4fe6-8cc1-4fe0387e8c63'
    WHERE `Id` = 'e572b070-82bd-47f0-b486-cc1b644b2d9e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '45891013-8681-46a1-9e24-49e0e06f96ad'
    WHERE `Id` = 'e5ac8272-7f9f-47c0-8e21-040fe3d242ed';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '523d1a63-7896-465d-a1fa-9e1919d3eacd'
    WHERE `Id` = 'e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '553cbc30-4f99-4313-bac3-064e081f7b03'
    WHERE `Id` = 'f517b79d-1fc4-4800-bcb8-ee0ca67dce1e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260623173808_SyncCurrentModelWithMySql')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260623173808_SyncCurrentModelWithMySql', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '5d70ce28-3b57-4f8e-adb5-517a68abc954'
    WHERE `Id` = '0c5e0418-46b3-4c6e-887e-0c182171ab11';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '24157a2c-e9f1-4f14-984a-aa1d1ddb61e1'
    WHERE `Id` = '4f43b487-3f8e-426d-9a46-048c7d07f7f9';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '149fafab-f8ca-429b-beca-f8deafcfdcb6'
    WHERE `Id` = '5f36c2f9-330a-492c-8ebf-65141782f2bb';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '2d702ff7-2e6a-4af1-abca-7171e3821169'
    WHERE `Id` = '6240e185-5c3a-410b-99d3-9767571fdf24';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '145db0c7-54f0-44d2-b144-b10bd039e713'
    WHERE `Id` = '67320cb2-92a2-4de7-971b-7e9e80244f4b';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'ab9c57d4-a21a-4e0b-9142-d152f6dfb1ca'
    WHERE `Id` = '9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '98687dd4-86b9-497a-a88a-6e4aa179bb8f'
    WHERE `Id` = 'b1477f6c-54ef-48d0-b24c-756b3a83b1a1';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '60299a0c-0571-407b-856b-e4de139428e8'
    WHERE `Id` = 'e572b070-82bd-47f0-b486-cc1b644b2d9e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '264de86e-4ce6-40ca-9b5a-b5971c476ded'
    WHERE `Id` = 'e5ac8272-7f9f-47c0-8e21-040fe3d242ed';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '7cfdff55-7a08-4081-ad04-5bee43030d79'
    WHERE `Id` = 'e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '8736ec5a-098e-42d5-890a-783b021e5c24'
    WHERE `Id` = 'f517b79d-1fc4-4800-bcb8-ee0ca67dce1e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624113820_FixWooImportRecordSchema_Final')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624113820_FixWooImportRecordSchema_Final', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
        `Id` BIGINT NOT NULL AUTO_INCREMENT,
        `SourceType` VARCHAR(64) NOT NULL,
        `SourceUrl` TEXT NULL,
        `ExternalId` VARCHAR(191) NULL,
        `Name` VARCHAR(512) NULL,
        `Status` VARCHAR(128) NULL,
        `RawJson` LONGTEXT NOT NULL,
        `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_khb_imported_woocommerce_records` PRIMARY KEY (`Id`),
        INDEX `IX_khb_imported_woocommerce_records_SourceType` (`SourceType`),
        INDEX `IX_khb_imported_woocommerce_records_ExternalId` (`ExternalId`)
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    UPDATE `khb_imported_woocommerce_records` SET `SourceType` = 'unknown' WHERE `SourceType` IS NULL OR `SourceType` = ''; 
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    UPDATE `khb_imported_woocommerce_records` SET `RawJson` = '{}' WHERE `RawJson` IS NULL OR `RawJson` = ''; 
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceType` VARCHAR(64) NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceUrl` TEXT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ExternalId` VARCHAR(191) NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Name` VARCHAR(512) NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Status` VARCHAR(128) NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `RawJson` LONGTEXT NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    SET @has := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_imported_woocommerce_records'
        AND COLUMN_NAME = 'ImportedAtUtc'
    );
    SET @ddl := IF(
      @has = 0,
      'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);',
      'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    SET @has := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_imported_woocommerce_records'
        AND COLUMN_NAME = 'ExternalId'
    );
    SET @ddl := IF(
      @has = 1,
      'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ExternalId` VARCHAR(191) NULL;',
      'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    SET @has := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_imported_woocommerce_records'
        AND COLUMN_NAME = 'SourceUrl'
    );
    SET @ddl := IF(
      @has = 1,
      'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceUrl` VARCHAR(1000) NULL;',
      'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    SET @has := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_imported_woocommerce_records'
        AND COLUMN_NAME = 'Name'
    );
    SET @ddl := IF(
      @has = 1,
      'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Name` VARCHAR(500) NULL;',
      'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN

    SET @has := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_imported_woocommerce_records'
        AND COLUMN_NAME = 'Status'
    );
    SET @ddl := IF(
      @has = 1,
      'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Status` VARCHAR(100) NULL;',
      'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624142000_FixWooImportedRecordsSchema')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624142000_FixWooImportedRecordsSchema', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624175028_RepairProductCsvImportSchema')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624175028_RepairProductCsvImportSchema', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624181512_RepairLegacyProductMainGroupName')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624181512_RepairLegacyProductMainGroupName', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_source_product` (
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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_category_map` (
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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_commodity` (
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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_package_type` (
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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_product_final` (
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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624230417_CreateKharbarchiProductWorkflowTables')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624230417_CreateKharbarchiProductWorkflowTables', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233105_AddSourceRowIdToKharbarchiSourceProduct')
BEGIN

    SET @tableExists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'khb_source_product'
    );
    SET @columnExists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'khb_source_product'
          AND COLUMN_NAME = 'SourceRowId'
    );
    SET @ddl := IF(
        @tableExists = 1 AND @columnExists = 0,
        'ALTER TABLE `khb_source_product` ADD COLUMN `SourceRowId` BIGINT NULL',
        'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233105_AddSourceRowIdToKharbarchiSourceProduct')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624233105_AddSourceRowIdToKharbarchiSourceProduct', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN

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

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260624233725_RebuildKharbarchiWorkflowTablesForCsvProcessor', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625131126_AddEnTaxonomicToKharbarchiMainGroups')
BEGIN

    SET @tableExists := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_product_main_groups'
    );
    SET @colExists := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'khb_product_main_groups'
        AND COLUMN_NAME = 'EnTaxonomic'
    );
    SET @ddl := IF(
      @tableExists = 1 AND @colExists = 0,
      'ALTER TABLE `khb_product_main_groups` ADD COLUMN `EnTaxonomic` VARCHAR(500) NULL AFTER `CategoryName`;',
      'SELECT 1'
    );
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625131126_AddEnTaxonomicToKharbarchiMainGroups')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260625131126_AddEnTaxonomicToKharbarchiMainGroups', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN

    SET @hasTable := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'khb_product_main_groups'
    );
    SET @hasColumn := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'khb_product_main_groups'
          AND COLUMN_NAME = 'EnTaxonomic'
    );

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN

    SET @ddl := IF(
        @hasTable = 1 AND @hasColumn = 0,
        'ALTER TABLE `khb_product_main_groups` ADD COLUMN `EnTaxonomic` VARCHAR(500) NULL AFTER `CategoryName`',
        'SELECT 1'
    );

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260625163821_AddEnTaxonomicToMainGroupsFinal')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260625163821_AddEnTaxonomicToMainGroupsFinal', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'gnr_Commodities'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'gnr_Commodities'
                  AND COLUMN_NAME = 'WooCommerceCommodityId'
            ),
            'ALTER TABLE `gnr_Commodities` ADD COLUMN `WooCommerceCommodityId` BIGINT NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'gnr_Categories'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'gnr_Categories'
                  AND COLUMN_NAME = 'WooCommerceCategoryId'
            ),
            'ALTER TABLE `gnr_Categories` ADD COLUMN `WooCommerceCategoryId` BIGINT NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_product_final'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_product_final'
                  AND COLUMN_NAME = 'SaleMode'
            ),
            'ALTER TABLE `khb_product_final` ADD COLUMN `SaleMode` VARCHAR(80) NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_product_final'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_product_final'
                  AND COLUMN_NAME = 'PriceCalculationBasis'
            ),
            'ALTER TABLE `khb_product_final` ADD COLUMN `PriceCalculationBasis` VARCHAR(80) NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_sale_products'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_sale_products'
                  AND COLUMN_NAME = 'SaleMode'
            ),
            'ALTER TABLE `khb_sale_products` ADD COLUMN `SaleMode` VARCHAR(80) NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_sale_products'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'khb_sale_products'
                  AND COLUMN_NAME = 'PriceCalculationBasis'
            ),
            'ALTER TABLE `khb_sale_products` ADD COLUMN `PriceCalculationBasis` VARCHAR(80) NULL',
            'SELECT 1'
        )
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '0c5e0418-46b3-4c6e-887e-0c182171ab11'
    WHERE `Id` = '0c5e0418-46b3-4c6e-887e-0c182171ab11';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '4f43b487-3f8e-426d-9a46-048c7d07f7f9'
    WHERE `Id` = '4f43b487-3f8e-426d-9a46-048c7d07f7f9';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '5f36c2f9-330a-492c-8ebf-65141782f2bb'
    WHERE `Id` = '5f36c2f9-330a-492c-8ebf-65141782f2bb';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '6240e185-5c3a-410b-99d3-9767571fdf24'
    WHERE `Id` = '6240e185-5c3a-410b-99d3-9767571fdf24';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '67320cb2-92a2-4de7-971b-7e9e80244f4b'
    WHERE `Id` = '67320cb2-92a2-4de7-971b-7e9e80244f4b';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = '9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c'
    WHERE `Id` = '9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'b1477f6c-54ef-48d0-b24c-756b3a83b1a1'
    WHERE `Id` = 'b1477f6c-54ef-48d0-b24c-756b3a83b1a1';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'e572b070-82bd-47f0-b486-cc1b644b2d9e'
    WHERE `Id` = 'e572b070-82bd-47f0-b486-cc1b644b2d9e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'e5ac8272-7f9f-47c0-8e21-040fe3d242ed'
    WHERE `Id` = 'e5ac8272-7f9f-47c0-8e21-040fe3d242ed';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52'
    WHERE `Id` = 'e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    UPDATE `sec_AspNetRoles` SET `ConcurrencyStamp` = 'f517b79d-1fc4-4800-bcb8-ee0ca67dce1e'
    WHERE `Id` = 'f517b79d-1fc4-4800-bcb8-ee0ca67dce1e';
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            COUNT(*) = 0,
            'CREATE UNIQUE INDEX `IX_gnr_Commodities_WooCommerceCommodityId` ON `gnr_Commodities` (`WooCommerceCommodityId`)',
            'SELECT 1'
        )
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'gnr_Commodities'
          AND INDEX_NAME = 'IX_gnr_Commodities_WooCommerceCommodityId'
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN

    SET @ddl := (
        SELECT IF(
            COUNT(*) = 0,
            'CREATE UNIQUE INDEX `IX_gnr_Categories_WooCommerceCategoryId` ON `gnr_Categories` (`WooCommerceCategoryId`)',
            'SELECT 1'
        )
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'gnr_Categories'
          AND INDEX_NAME = 'IX_gnr_Categories_WooCommerceCategoryId'
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    PREPARE stmt FROM @ddl;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    EXECUTE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    DEALLOCATE PREPARE stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703164535_ReconcileAppDbContextModel20260703')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260703164535_ReconcileAppDbContextModel20260703', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUsers'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetusers'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetUsers` TO `sec_aspnetusers`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetRoles'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetroles'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetRoles` TO `sec_aspnetroles`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserRoles'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserroles'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetUserRoles` TO `sec_aspnetuserroles`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserClaims'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserclaims'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetUserClaims` TO `sec_aspnetuserclaims`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserLogins'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserlogins'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetUserLogins` TO `sec_aspnetuserlogins`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetRoleClaims'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetroleclaims'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetRoleClaims` TO `sec_aspnetroleclaims`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserTokens'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetusertokens'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sec_AspNetUserTokens` TO `sec_aspnetusertokens`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Brands'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_brands'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_Brands` TO `gnr_brands`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Categories'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_categories'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_Categories` TO `gnr_categories`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Commodities'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_commodities'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_Commodities` TO `gnr_commodities`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Products'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_products'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_Products` TO `gnr_products`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductVariants'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productvariants'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_ProductVariants` TO `gnr_productvariants`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductTags'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_producttags'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_ProductTags` TO `gnr_producttags`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductProductTags'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productproducttags'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_ProductProductTags` TO `gnr_productproducttags`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductSpecDefinitions'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productspecdefinitions'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_ProductSpecDefinitions` TO `gnr_productspecdefinitions`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductSpecValues'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productspecvalues'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `gnr_ProductSpecValues` TO `gnr_productspecvalues`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'cbi_Customers'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'cbi_customers'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `cbi_Customers` TO `cbi_customers`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_Orders'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_orders'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `com_Orders` TO `com_orders`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_OrderItems'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_orderitems'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `com_OrderItems` TO `com_orderitems`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_PriceProposals'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_priceproposals'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `wf_PriceProposals` TO `wf_priceproposals`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_InventoryProposals'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_inventoryproposals'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `wf_InventoryProposals` TO `wf_inventoryproposals`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_ApprovalAuditLogs'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_approvalauditlogs'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `wf_ApprovalAuditLogs` TO `wf_approvalauditlogs`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sync_OutboxMessages'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sync_outboxmessages'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sync_OutboxMessages` TO `sync_outboxmessages`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_WooCommerceSyncLogs'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_woocommercesynclogs'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sup_WooCommerceSyncLogs` TO `sup_woocommercesynclogs`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_GatewayPaymentReceipts'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_gatewaypaymentreceipts'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `sup_GatewayPaymentReceipts` TO `sup_gatewaypaymentreceipts`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_WooCommerceOrderSnapshots'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_woocommerceordersnapshots'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `ord_WooCommerceOrderSnapshots` TO `ord_woocommerceordersnapshots`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_WooCommerceOrderItemSnapshots'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_woocommerceorderitemsnapshots'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `ord_WooCommerceOrderItemSnapshots` TO `ord_woocommerceorderitemsnapshots`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_BarookPaymentSessions'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_barookpaymentsessions'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `pay_BarookPaymentSessions` TO `pay_barookpaymentsessions`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_legacy_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_ManualPaymentReceipts'
    );
    SET @khb_canonical_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_manualpaymentreceipts'
    );
    SET @khb_sql = IF(
      @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
      'RENAME TABLE `pay_ManualPaymentReceipts` TO `pay_manualpaymentreceipts`',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    CREATE TABLE IF NOT EXISTS `khb_product_change_log` (
      `Id` BIGINT NOT NULL AUTO_INCREMENT,
      `ProductId` BIGINT NOT NULL,
      `ChangeType` VARCHAR(100) NOT NULL,
      `Summary` VARCHAR(1000) NULL,
      `Payload` LONGTEXT NULL,
      `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
      PRIMARY KEY (`Id`),
      KEY `IX_khb_product_change_log_ProductId` (`ProductId`)
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

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
    ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_category_map'
        AND BINARY COLUMN_NAME = BINARY 'CategoryName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_category_map` ADD COLUMN `CategoryName` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_commodity'
        AND BINARY COLUMN_NAME = BINARY 'CommodityName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_commodity` ADD COLUMN `CommodityName` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_package_type` ADD COLUMN `PackageTitle` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_package_type` ADD COLUMN `PackagingPricePerPack` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'WooPackageId'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_package_type` ADD COLUMN `WooPackageId` BIGINT NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `ProductName` VARCHAR(700) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `ProductEnglishName` VARCHAR(700) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'MainProductName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `MainProductName` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'CategoryName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `CategoryName` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `BrandName` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `BrandEnglishName` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'UnitWeightKg'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `UnitWeightKg` DECIMAL(18,6) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `KgCashPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_source_product` ADD COLUMN `KgCreditPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'MainGroupId'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `MainGroupId` BIGINT NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'CategorySourceKey'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `CategorySourceKey` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'CommoditySourceKey'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `CommoditySourceKey` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackageSourceKey'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `PackageSourceKey` VARCHAR(500) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `ProductName` VARCHAR(700) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `ProductEnglishName` VARCHAR(700) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `BrandName` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `BrandEnglishName` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `PackageTitle` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `PackagingPricePerPack` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `KgCashPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `KgCreditPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SaleCashPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `SaleCashPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SaleCreditPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `SaleCreditPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BuyCashPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `BuyCashPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BuyCreditPrice'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `BuyCreditPrice` DECIMAL(18,2) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'CatalogVisibility'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `CatalogVisibility` VARCHAR(50) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `WooPayloadJson` LONGTEXT NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BulkWeightKg'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `BulkWeightKg` DECIMAL(18,6) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'MinPurchaseKg'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `MinPurchaseKg` DECIMAL(18,6) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ImageTag'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `ImageTag` VARCHAR(300) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SaleMode'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `SaleMode` VARCHAR(80) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PriceCalculationBasis'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_final` ADD COLUMN `PriceCalculationBasis` VARCHAR(80) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'QueueStatus'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `QueueStatus` VARCHAR(50) NOT NULL DEFAULT ''pending''',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'ActionType'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `ActionType` VARCHAR(50) NOT NULL DEFAULT ''upsert''',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'SKU'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `SKU` VARCHAR(191) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'ProductSlug'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `ProductSlug` VARCHAR(700) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'WooProductId'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `WooProductId` BIGINT NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `WooPayloadJson` LONGTEXT NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'JobId'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `JobId` CHAR(36) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'TryCount'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD COLUMN `TryCount` INT NOT NULL DEFAULT 0',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
        AND BINARY COLUMN_NAME = BINARY 'Slug'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN `Slug` VARCHAR(255) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_column_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
        AND BINARY COLUMN_NAME = BINARY 'Title'
    );
    SET @khb_sql = IF(
      @khb_column_exists = 0,
      'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN `Title` VARCHAR(512) NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_category_map'
        AND BINARY COLUMN_NAME = BINARY 'CategoryName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_category_map'
        AND BINARY COLUMN_NAME = BINARY 'CategoryNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_category_map` SET `CategoryName` = `CategoryNameFa` WHERE `CategoryName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_commodity'
        AND BINARY COLUMN_NAME = BINARY 'CommodityName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_commodity'
        AND BINARY COLUMN_NAME = BINARY 'CommodityNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_commodity` SET `CommodityName` = `CommodityNameFa` WHERE `CommodityName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackageNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_package_type` SET `PackageTitle` = `PackageNameFa` WHERE `PackageTitle` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPackToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_package_type` SET `PackagingPricePerPack` = `PackagingPricePerPackToman` WHERE `PackagingPricePerPack` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `ProductName` = `ProductNameFa` WHERE `ProductName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'ProductNameEn'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `ProductEnglishName` = `ProductNameEn` WHERE `ProductEnglishName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'MainProductName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'MainProductNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `MainProductName` = `MainProductNameFa` WHERE `MainProductName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'CategoryName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'CategoryNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `CategoryName` = `CategoryNameFa` WHERE `CategoryName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `BrandName` = `BrandNameFa` WHERE `BrandName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'BrandNameEn'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `BrandEnglishName` = `BrandNameEn` WHERE `BrandEnglishName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'UnitWeightKg'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'PackageOneKg'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `UnitWeightKg` = `PackageOneKg` WHERE `UnitWeightKg` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPriceToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `KgCashPrice` = `KgCashPriceToman` WHERE `KgCashPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPriceToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_source_product` SET `KgCreditPrice` = `KgCreditPriceToman` WHERE `KgCreditPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `ProductName` = `ProductNameFa` WHERE `ProductName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'ProductNameEn'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `ProductEnglishName` = `ProductNameEn` WHERE `ProductEnglishName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `BrandName` = `BrandNameFa` WHERE `BrandName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BrandNameEn'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `BrandEnglishName` = `BrandNameEn` WHERE `BrandEnglishName` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackageNameFa'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `PackageTitle` = `PackageNameFa` WHERE `PackageTitle` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPackToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `PackagingPricePerPack` = `PackagingPricePerPackToman` WHERE `PackagingPricePerPack` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCashPriceToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `KgCashPrice` = `KgCashPriceToman` WHERE `KgCashPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'KgCreditPriceToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `KgCreditPrice` = `KgCreditPriceToman` WHERE `KgCreditPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SaleCashPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SalePriceCashToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `SaleCashPrice` = `SalePriceCashToman` WHERE `SaleCashPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SaleCreditPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'SalePriceCreditToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `SaleCreditPrice` = `SalePriceCreditToman` WHERE `SaleCreditPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BuyCashPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PurchasePriceCashToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `BuyCashPrice` = `PurchasePriceCashToman` WHERE `BuyCashPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'BuyCreditPrice'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY COLUMN_NAME = BINARY 'PurchasePriceCreditToman'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_final` SET `BuyCreditPrice` = `PurchasePriceCreditToman` WHERE `BuyCreditPrice` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'QueueStatus'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'SyncStatus'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_update_queue` SET `QueueStatus` = `SyncStatus` WHERE `QueueStatus` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'PayloadJson'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_update_queue` SET `WooPayloadJson` = `PayloadJson` WHERE `WooPayloadJson` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_target_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'WooProductId'
    );
    SET @khb_source_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY COLUMN_NAME = BINARY 'WooObjectId'
    );
    SET @khb_sql = IF(
      @khb_target_exists = 1 AND @khb_source_exists = 1,
      'UPDATE `khb_product_update_queue` SET `WooProductId` = `WooObjectId` WHERE `WooProductId` IS NULL',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_index_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
        AND BINARY INDEX_NAME = BINARY 'IX_khb_package_type_WooPackageId'
    );
    SET @khb_sql = IF(
      @khb_index_exists = 0,
      'ALTER TABLE `khb_package_type` ADD INDEX `IX_khb_package_type_WooPackageId` (`WooPackageId`)',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_index_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY INDEX_NAME = BINARY 'IX_khb_product_final_ProductSlug'
    );
    SET @khb_sql = IF(
      @khb_index_exists = 0,
      'ALTER TABLE `khb_product_final` ADD INDEX `IX_khb_product_final_ProductSlug` (`ProductSlug`(191))',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_index_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
        AND BINARY INDEX_NAME = BINARY 'IX_khb_product_final_WooProductId'
    );
    SET @khb_sql = IF(
      @khb_index_exists = 0,
      'ALTER TABLE `khb_product_final` ADD INDEX `IX_khb_product_final_WooProductId` (`WooProductId`)',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_index_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
        AND BINARY INDEX_NAME = BINARY 'IX_khb_product_update_queue_JobId'
    );
    SET @khb_sql = IF(
      @khb_index_exists = 0,
      'ALTER TABLE `khb_product_update_queue` ADD INDEX `IX_khb_product_update_queue_JobId` (`JobId`)',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN

    SET @khb_index_exists = (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
      WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
        AND BINARY INDEX_NAME = BINARY 'UX_khb_imported_woocommerce_records_Source_External'
    );
    SET @khb_duplicate_count = (
      SELECT COUNT(*) FROM (
        SELECT 1 FROM `khb_imported_woocommerce_records`
        WHERE `ExternalId` IS NOT NULL
        GROUP BY `SourceType`, `ExternalId`
        HAVING COUNT(*) > 1
      ) khb_duplicates
    );
    SET @khb_sql = IF(
      @khb_index_exists = 0 AND @khb_duplicate_count = 0,
      'ALTER TABLE `khb_imported_woocommerce_records` ADD UNIQUE INDEX `UX_khb_imported_woocommerce_records_Source_External` (`SourceType`, `ExternalId`)',
      'SELECT 1'
    );
    PREPARE khb_stmt FROM @khb_sql;
    EXECUTE khb_stmt;
    DEALLOCATE PREPARE khb_stmt;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221056_AlignCanonicalKharbarchiWorkflow20260704')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260703221056_AlignCanonicalKharbarchiWorkflow20260704', '10.0.9');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260703221623_FinalizeCanonicalKharbarchiModel20260704')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260703221623_FinalizeCanonicalKharbarchiModel20260704', '10.0.9');
END;

COMMIT;

