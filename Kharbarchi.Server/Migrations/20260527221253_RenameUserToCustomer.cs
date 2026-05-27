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
            migrationBuilder.UpdateData(
                  schema: "sec", // using your schema name from the error
                  table: "AspNetRoles",
                  keyColumn: "Id",
                  keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                  columns: new[] { "Name", "NormalizedName" },
                  values: new object[] { "Customer", "CUSTOMER" });
        
            migrationBuilder.InsertData(
                  schema: "sec",
                  table: "AspNetRoles",
                  columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                  values: new object[,]
                  {
                      { "9a4918f4-604a-4e2b-be24-5d5d852ec6a2", "e98e4f5a-5bc4-4845-8b38-dbb4eb5b7964", "AdminSeller", "ADMINSELLER" },
                      { "db563459-d897-400f-87d2-747d86f2b236", "a71f00a4-3221-4d32-8418-df8db3748c0f", "Seller", "SELLER" },
                  });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

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

           
            migrationBuilder.UpdateData(
                 schema: "sec",
                 table: "AspNetRoles",
                 keyColumn: "Id",
                 keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                 columns: new[] { "Name", "NormalizedName" },
                 values: new object[] { "User", "USER" });
        }
    }
}
