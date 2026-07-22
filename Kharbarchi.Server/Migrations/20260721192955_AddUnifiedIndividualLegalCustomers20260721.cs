using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedIndividualLegalCustomers20260721 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllowedSpending",
                table: "cbi_customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCategory",
                table: "cbi_customers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreditExpiresAtUtc",
                table: "cbi_customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerType",
                table: "cbi_customers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Legal");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "cbi_customers",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefinedByDistribution",
                table: "cbi_customers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "cbi_customers",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                table: "cbi_customers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "cbi_customers",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceRemainingCredit",
                table: "cbi_customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreName",
                table: "cbi_customers",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cbi_customers_NationalCode",
                table: "cbi_customers",
                column: "NationalCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cbi_customers_NationalCode",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "AllowedSpending",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "BusinessCategory",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "CreditExpiresAtUtc",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "IsDefinedByDistribution",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "SourceRemainingCredit",
                table: "cbi_customers");

            migrationBuilder.DropColumn(
                name: "StoreName",
                table: "cbi_customers");
        }
    }
}
