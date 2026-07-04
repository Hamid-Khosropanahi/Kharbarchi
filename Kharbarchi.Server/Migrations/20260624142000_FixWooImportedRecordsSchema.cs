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
    `ExternalId` VARCHAR(191) NULL,
    `Slug` VARCHAR(255) NULL,
    `Title` VARCHAR(512) NULL,
    `RawJson` LONGTEXT NOT NULL,
    `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `SourceUrl` TEXT NULL,
    `Name` VARCHAR(512) NULL,
    `Status` VARCHAR(128) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_khb_imported_woocommerce_records` PRIMARY KEY (`Id`),
    UNIQUE INDEX `UX_khb_imported_woocommerce_records_Source_External` (`SourceType`, `ExternalId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

        // MySQL DDL is not fully transactional. A previous failed attempt can therefore leave
        // the table present but missing one or more columns. Add every mapped column before any
        // UPDATE or MODIFY so rerunning this unapplied migration is safe.
        AddColumnIfMissing(migrationBuilder, "SourceType", "`SourceType` VARCHAR(64) NULL");
        AddColumnIfMissing(migrationBuilder, "ExternalId", "`ExternalId` VARCHAR(191) NULL");
        AddColumnIfMissing(migrationBuilder, "Slug", "`Slug` VARCHAR(255) NULL");
        AddColumnIfMissing(migrationBuilder, "Title", "`Title` VARCHAR(512) NULL");
        AddColumnIfMissing(migrationBuilder, "RawJson", "`RawJson` LONGTEXT NULL");
        AddColumnIfMissing(
            migrationBuilder,
            "ImportedAtUtc",
            "`ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)");
        AddColumnIfMissing(migrationBuilder, "SourceUrl", "`SourceUrl` TEXT NULL");
        AddColumnIfMissing(migrationBuilder, "Name", "`Name` VARCHAR(512) NULL");
        AddColumnIfMissing(migrationBuilder, "Status", "`Status` VARCHAR(128) NULL");
        AddColumnIfMissing(
            migrationBuilder,
            "CreatedAtUtc",
            "`CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)");

        migrationBuilder.Sql(@"UPDATE `khb_imported_woocommerce_records` SET `SourceType` = 'unknown' WHERE `SourceType` IS NULL OR `SourceType` = ''; ");
        migrationBuilder.Sql(@"UPDATE `khb_imported_woocommerce_records` SET `RawJson` = '{}' WHERE `RawJson` IS NULL OR `RawJson` = ''; ");

        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceType` VARCHAR(64) NOT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `SourceUrl` TEXT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ExternalId` VARCHAR(191) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Slug` VARCHAR(255) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Title` VARCHAR(512) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Name` VARCHAR(512) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `Status` VARCHAR(128) NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `RawJson` LONGTEXT NOT NULL;");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);");
        migrationBuilder.Sql(@"ALTER TABLE `khb_imported_woocommerce_records` MODIFY COLUMN `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
    }

    private static void AddColumnIfMissing(
        MigrationBuilder migrationBuilder,
        string columnName,
        string definition)
    {
        var escapedDefinition = definition.Replace("'", "''", StringComparison.Ordinal);
        migrationBuilder.Sql($@"
SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
    AND BINARY COLUMN_NAME = BINARY '{columnName}'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN {escapedDefinition}',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;");
    }
}
