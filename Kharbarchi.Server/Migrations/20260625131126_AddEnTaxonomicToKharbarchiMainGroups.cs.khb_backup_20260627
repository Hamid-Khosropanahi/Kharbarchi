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
ALTER TABLE `khb_product_main_groups`
ADD COLUMN `EnTaxonomic` VARCHAR(500) NULL AFTER `CategoryName`;
");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `khb_product_main_groups`
DROP COLUMN `EnTaxonomic`;
");
        }
    }
}
