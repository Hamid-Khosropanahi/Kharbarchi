using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceRowIdToKharbarchiSourceProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `khb_source_product`
ADD COLUMN `SourceRowId` BIGINT NULL;
");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
