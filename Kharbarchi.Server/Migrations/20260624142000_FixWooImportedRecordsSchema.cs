using Kharbarchi.Server.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260624142000_FixWooImportedRecordsSchema")]
public partial class FixWooImportedRecordsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceType` VARCHAR(64) NOT NULL,
    `SourceUrl` TEXT NULL,
    `ExternalId` VARCHAR(191) NULL,
    `Name` VARCHAR(512) NULL,
    `Status` VARCHAR(128) NULL,
    `RawJson` LONGTEXT NOT NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `PK_khb_imported_woocommerce_records` PRIMARY KEY (`Id`),
    INDEX `IX_khb_imported_woocommerce_records_SourceType` (`SourceType`),
    INDEX `IX_khb_imported_woocommerce_records_ExternalId` (`ExternalId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        migrationBuilder.Sql(@"UPDATE `khb_imported_woocommerce_records` SET `SourceType` = 'unknown' WHERE `SourceType` IS NULL OR `SourceType` = ''; ");
        migrationBuilder.Sql(@"UPDATE `khb_imported_woocommerce_records` SET `RawJson` = '{}' WHERE `RawJson` IS NULL OR `RawJson` = ''; ");

        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceType` VARCHAR(64) NOT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceUrl` TEXT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ExternalId` VARCHAR(191) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Name` VARCHAR(512) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Status` VARCHAR(128) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `RawJson` LONGTEXT NOT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");


                // Apply safe conditional ALTERs: only run MODIFY if the column exists.
                migrationBuilder.Sql(@"
SET @has := (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'khb_imported_woocommerce_records'
    AND COLUMN_NAME = 'ImportedAtUtc'
);
SET @ddl := IF(
  @has = 1,
  'ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);',
  'SELECT 1'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

                migrationBuilder.Sql(@"
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
");

                migrationBuilder.Sql(@"
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
");

                migrationBuilder.Sql(@"
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
");

                migrationBuilder.Sql(@"
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
");






    }

    protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
}
