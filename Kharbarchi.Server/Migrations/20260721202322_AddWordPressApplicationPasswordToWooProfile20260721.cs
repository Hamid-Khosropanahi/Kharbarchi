using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddWordPressApplicationPasswordToWooProfile20260721 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProtectedWordPressApplicationPassword",
                table: "khb_woocommerce_connection_profiles",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordPressUsername",
                table: "khb_woocommerce_connection_profiles",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProtectedWordPressApplicationPassword",
                table: "khb_woocommerce_connection_profiles");

            migrationBuilder.DropColumn(
                name: "WordPressUsername",
                table: "khb_woocommerce_connection_profiles");
        }
    }
}
