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
    @hasColumn = 0,
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
            migrationBuilder.Sql(@"
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
    @hasColumn = 1,
    'ALTER TABLE `khb_product_main_groups` DROP COLUMN `EnTaxonomic`',
    'SELECT 1'
);
");

            migrationBuilder.Sql(@"PREPARE stmt FROM @ddl;");
            migrationBuilder.Sql(@"EXECUTE stmt;");
            migrationBuilder.Sql(@"DEALLOCATE PREPARE stmt;");
        }
    }
}
