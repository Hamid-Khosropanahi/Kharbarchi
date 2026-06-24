using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class RepairLegacyProductMainGroupName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `khb_product_main_groups` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `SourceKey` VARCHAR(500) NULL,
    `Name` VARCHAR(300) NULL,
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
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'Name', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'MainProductName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'MainProductSlug', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'CategoryName', 'VARCHAR(300) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'EnTaxonomic', 'VARCHAR(200) NULL');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'CreatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)');
CALL `khb_add_column_if_missing`('khb_product_main_groups', 'UpdatedAtUtc', 'DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)');
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS `khb_add_column_if_missing`;
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups`
SET `MainProductName` = COALESCE(NULLIF(`MainProductName`, ''), NULLIF(`Name`, ''), 'بدون گروه')
WHERE `MainProductName` IS NULL OR `MainProductName` = '';
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups`
SET `Name` = COALESCE(NULLIF(`Name`, ''), NULLIF(`MainProductName`, ''), 'بدون گروه')
WHERE `Name` IS NULL OR `Name` = '';
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups`
SET `MainProductSlug` = COALESCE(NULLIF(`MainProductSlug`, ''), REPLACE(`MainProductName`, ' ', '-'), CONCAT('main-', `Id`))
WHERE `MainProductSlug` IS NULL OR `MainProductSlug` = '';
");

            migrationBuilder.Sql(@"
UPDATE `khb_product_main_groups`
SET `SourceKey` = LOWER(CONCAT(
    'main:',
    COALESCE(NULLIF(`EnTaxonomic`, ''), 'category'),
    ':',
    COALESCE(NULLIF(`MainProductSlug`, ''), CONCAT('main-', `Id`)),
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
MODIFY COLUMN `Name` VARCHAR(300) NULL;
");

            migrationBuilder.Sql(@"
ALTER TABLE `khb_product_main_groups`
MODIFY COLUMN `SourceKey` VARCHAR(500) NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty.
            // This migration repairs a legacy product main group schema.
        }
    }
}
