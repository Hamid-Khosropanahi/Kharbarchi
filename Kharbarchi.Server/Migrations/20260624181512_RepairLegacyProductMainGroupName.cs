using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    public partial class RepairLegacyProductMainGroupName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Neutralized because hardcoded workflow migrations rebuild generated staging tables safely.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // KHB-SAFE: rollback intentionally does not drop data-bearing tables, columns, or indexes.
        }
    }
}
