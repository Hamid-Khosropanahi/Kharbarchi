using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEnTaxonomicToKharbarchiMainGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
