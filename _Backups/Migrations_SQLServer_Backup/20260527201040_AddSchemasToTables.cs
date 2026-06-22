using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemasToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sec");

            migrationBuilder.EnsureSchema(
                name: "GNR");

            migrationBuilder.EnsureSchema(
                name: "CBI");

            migrationBuilder.EnsureSchema(
                name: "COM");

            migrationBuilder.RenameTable(
                name: "ProductVariants",
                newName: "ProductVariants",
                newSchema: "GNR");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "GNR");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Orders",
                newSchema: "COM");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItems",
                newSchema: "COM");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Customers",
                newSchema: "CBI");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Categories",
                newSchema: "GNR");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "sec");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "sec");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "sec");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "sec");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "sec");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "sec");

            migrationBuilder.RenameTable(
               name: "AspNetUsers",
               schema:null,
               newName: "AspNetUsers",
               newSchema: "sec");


            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    schema: "sec",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "sec");

            migrationBuilder.RenameTable(
                name: "ProductVariants",
                schema: "GNR",
                newName: "ProductVariants");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "GNR",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "COM",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                schema: "COM",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "CBI",
                newName: "Customers");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "GNR",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "sec",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "sec",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "sec",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "sec",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "sec",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "sec",
                newName: "AspNetRoleClaims");
        }
    }
}
