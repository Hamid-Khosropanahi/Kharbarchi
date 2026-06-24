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


                migrationBuilder.Sql(@"
        ALTER TABLE khb_imported_woocommerce_records
        MODIFY COLUMN ImportedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);
        ");

                migrationBuilder.Sql(@"
        ALTER TABLE khb_imported_woocommerce_records
        MODIFY COLUMN ExternalId VARCHAR(191) NULL;
        ");

                migrationBuilder.Sql(@"
        ALTER TABLE khb_imported_woocommerce_records
        MODIFY COLUMN SourceUrl VARCHAR(1000) NULL;
        ");

                migrationBuilder.Sql(@"
        ALTER TABLE khb_imported_woocommerce_records
        MODIFY COLUMN Name VARCHAR(500) NULL;
        ");

                migrationBuilder.Sql(@"
        ALTER TABLE khb_imported_woocommerce_records
        MODIFY COLUMN Status VARCHAR(100) NULL;
        ");






    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This migration repairs an import/audit table used for raw WooCommerce snapshots.
        // Do not drop or make columns NOT NULL in Down, because that could destroy imported data.
    }
}
