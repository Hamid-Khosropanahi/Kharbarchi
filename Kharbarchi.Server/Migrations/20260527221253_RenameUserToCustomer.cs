using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "sec",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6240e185-5c3a-410b-99d3-9767571fdf24", null, "Customer", "CUSTOMER" },
                    { "9a4918f4-604a-4e2b-be24-5d5d852ec6a2", "e98e4f5a-5bc4-4845-8b38-dbb4eb5b7964", "AdminSeller", "ADMINSELLER" },
                    { "db563459-d897-400f-87d2-747d86f2b236", "a71f00a4-3221-4d32-8418-df8db3748c0f", "Seller", "SELLER" },
                    { "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "sec",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24");

            migrationBuilder.DeleteData(
                schema: "sec",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9a4918f4-604a-4e2b-be24-5d5d852ec6a2");

            migrationBuilder.DeleteData(
                schema: "sec",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "db563459-d897-400f-87d2-747d86f2b236");

            migrationBuilder.DeleteData(
                schema: "sec",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e");
        }
    }
}
