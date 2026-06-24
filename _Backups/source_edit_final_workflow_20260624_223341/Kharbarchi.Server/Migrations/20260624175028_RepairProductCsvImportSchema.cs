using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class RepairProductCsvImportSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "5a39a5fe-74ec-4f21-ab70-ba034ed22164");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "0de36bfa-d6a1-4855-93e0-f0caf8d29f41");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "fffea4ea-ce28-44a9-9270-518acaf60b16");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "02c62f06-be41-4fba-aa42-899f90b32b49");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "8f848310-4aad-4370-a090-ab874f96d034");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "d810f2ef-245d-4869-b950-63b3f65064d1");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "958902a5-0a7e-488d-928e-3370c760f164");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "4ae9ecf7-4cb2-43a0-b9ac-7035189000a3");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "0a15db0a-a778-45ab-b829-58459ed603bb");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "588def6d-ce8b-4ffa-b223-997f617cacae");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "ed32905b-22e2-4f13-a10f-9355100fd320");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_main_groups` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NULL,
    `MainProductName` VARCHAR(300) NULL,
    `MainProductSlug` VARCHAR(300) NULL,
    `CategoryName` VARCHAR(300) NULL,
    `EnTaxonomic` VARCHAR(200) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_sale_products` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `MainGroupId` BIGINT NULL,
    `ProductSlug` VARCHAR(255) NULL,
    `ProductSku` VARCHAR(191) NULL,
    `ProductName` VARCHAR(500) NULL,
    `MainProductName` VARCHAR(300) NULL,
    `CategoryName` VARCHAR(300) NULL,
    `EnTaxonomic` VARCHAR(200) NULL,
    `PackageName` VARCHAR(200) NULL,
    `PackageGroup` VARCHAR(50) NULL,
    `UnitWeightKg` DECIMAL(18,4) NULL,
    `PacksPerCarton` INT NULL,
    `PackagingPricePerPack` DECIMAL(18,2) NULL,
    `KgCashPrice` DECIMAL(18,2) NULL,
    `KgCreditPrice` DECIMAL(18,2) NULL,
    `SalePriceCash` DECIMAL(18,2) NULL,
    `SalePriceInstallment` DECIMAL(18,2) NULL,
    `PurchasePriceCash` DECIMAL(18,2) NULL,
    `PurchasePriceInstallment` DECIMAL(18,2) NULL,
    `ShortDescription` TEXT NULL,
    `FullDescription` LONGTEXT NULL,
    `ImageUrl` VARCHAR(1000) NULL,
    `Status` VARCHAR(50) NULL,
    `CatalogVisibility` VARCHAR(50) NULL,
    `SourceRowHash` CHAR(64) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS `khb_add_column_if_missing`;
");

            migrationBuilder.Sql(@"
CREATE PROCEDURE `khb_add_column_if_missing`(
    IN p_table_name VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_column_definition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = p_table_name
          AND COLUMN_NAME = p_column_name
    ) THEN
        SET @khb_sql = CONCAT(
            'ALTER TABLE `', p_table_name, '` ',
            'ADD COLUMN `', p_column_name, '` ',
            p_column_definition
        );

        PREPARE khb_stmt FROM @khb_sql;
        EXECUTE khb_stmt;
        DEALLOCATE PREPARE khb_stmt;
    END IF;
END
");

            migrationBuilder.Sql(@"
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'SourceKey', 'VARCHAR(500) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'MainProductName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'MainProductSlug', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'CategoryName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'EnTaxonomic', 'VARCHAR(200) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'CreatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'UpdatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)');

CALL `khb_add_column_if_missing`('khb_sale_products', 'MainGroupId', 'BIGINT NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'ProductSlug', 'VARCHAR(255) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'ProductSku', 'VARCHAR(191) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'ProductName', 'VARCHAR(500) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'MainProductName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'CategoryName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'EnTaxonomic', 'VARCHAR(200) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PackageName', 'VARCHAR(200) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PackageGroup', 'VARCHAR(50) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'UnitWeightKg', 'DECIMAL(18,4) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PacksPerCarton', 'INT NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PackagingPricePerPack', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'KgCashPrice', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'KgCreditPrice', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'SalePriceCash', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'SalePriceInstallment', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PurchasePriceCash', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'PurchasePriceInstallment', 'DECIMAL(18,2) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'ShortDescription', 'TEXT NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'FullDescription', 'LONGTEXT NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'ImageUrl', 'VARCHAR(1000) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'Status', 'VARCHAR(50) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'CatalogVisibility', 'VARCHAR(50) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'SourceRowHash', 'CHAR(64) NULL');
CALL `khb_add_column_if_missing`('khb_sale_products', 'CreatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)');
CALL `khb_add_column_if_missing`('khb_sale_products', 'UpdatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)');
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS `khb_add_column_if_missing`;
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups`
SET `SourceKey` = LOWER(CONCAT(
    'main:',
    COALESCE(NULLIF(`EnTaxonomic`, ''), 'category'),
    ':',
    COALESCE(NULLIF(`MainProductSlug`, ''), REPLACE(COALESCE(`MainProductName`, 'main'), ' ', '-')),
    ':',
    `Id`
))
WHERE `SourceKey` IS NULL OR `SourceKey` = '';
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups` g
JOIN (
    SELECT `SourceKey`
    FROM `khb_product_main_groups`
    WHERE `SourceKey` IS NOT NULL AND `SourceKey` <> ''
    GROUP BY `SourceKey`
    HAVING COUNT(*) > 1
) d ON d.`SourceKey` = g.`SourceKey`
SET g.`SourceKey` = CONCAT(g.`SourceKey`, ':', g.`Id`);
");

            migrationBuilder.Sql(@"
ALTER TABLE `khb_product_main_groups`
MODIFY COLUMN `SourceKey` VARCHAR(500) NOT NULL;
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS `khb_add_index_if_missing`;
");

            migrationBuilder.Sql(@"
CREATE PROCEDURE `khb_add_index_if_missing`(
    IN p_table_name VARCHAR(64),
    IN p_index_name VARCHAR(128),
    IN p_index_definition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = p_table_name
          AND INDEX_NAME = p_index_name
    ) THEN
        SET @khb_idx_sql = CONCAT(
            'ALTER TABLE `', p_table_name, '` ',
            'ADD ', p_index_definition
        );

        PREPARE khb_idx_stmt FROM @khb_idx_sql;
        EXECUTE khb_idx_stmt;
        DEALLOCATE PREPARE khb_idx_stmt;
    END IF;
END
");

            migrationBuilder.Sql(@"
CALL `khb_add_index_if_missing`(
    'khb_product_main_groups',
    'UX_khb_product_main_groups_SourceKey',
    'UNIQUE KEY `UX_khb_product_main_groups_SourceKey` (`SourceKey`)'
);
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS `khb_add_index_if_missing`;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "5d70ce28-3b57-4f8e-adb5-517a68abc954");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "24157a2c-e9f1-4f14-984a-aa1d1ddb61e1");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "149fafab-f8ca-429b-beca-f8deafcfdcb6");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "2d702ff7-2e6a-4af1-abca-7171e3821169");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "145db0c7-54f0-44d2-b144-b10bd039e713");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "ab9c57d4-a21a-4e0b-9142-d152f6dfb1ca");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "98687dd4-86b9-497a-a88a-6e4aa179bb8f");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "60299a0c-0571-407b-856b-e4de139428e8");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "264de86e-4ce6-40ca-9b5a-b5971c476ded");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "7cfdff55-7a08-4081-ad04-5bee43030d79");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "8736ec5a-098e-42d5-890a-783b021e5c24");
        }
    }
}
