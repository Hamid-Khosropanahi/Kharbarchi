using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEnTaxonomicToMainGroupsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
SET @ddl := IF(
    @hasTable = 1 AND @hasColumn = 0,
    'ALTER TABLE `khb_product_main_groups` ADD COLUMN `EnTaxonomic` VARCHAR(500) NULL AFTER `CategoryName`',
    'SELECT 1'
);
");

            migrationBuilder.Sql(@"PREPARE stmt FROM @ddl;");
            migrationBuilder.Sql(@"EXECUTE stmt;");
            migrationBuilder.Sql(@"DEALLOCATE PREPARE stmt;");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
