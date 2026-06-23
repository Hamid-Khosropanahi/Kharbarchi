using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModelWithMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "3a3f068b-ae89-4dcd-9020-3dfcb889d814");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "be2ca41b-2f03-4ba5-97ef-faf79467ef3c");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "1da09220-d3b7-473b-8e04-9922b6806f34");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "876fdd6e-b88f-4967-a6c8-7005462ca8ee");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "ca0e92e4-4676-4c47-ba32-c916a32b3e20");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "9c6142f8-6ed7-4502-aeb0-6f44b0987a17");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "b102ddc1-b72c-40be-99ec-cce6d718544b");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "231e6f04-589f-4fe6-8cc1-4fe0387e8c63");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "45891013-8681-46a1-9e24-49e0e06f96ad");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "523d1a63-7896-465d-a1fa-9e1919d3eacd");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "553cbc30-4f99-4313-bac3-064e081f7b03");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c5e0418-46b3-4c6e-887e-0c182171ab11",
                column: "ConcurrencyStamp",
                value: "d8b522b7-bf1b-4acb-ab32-dac6ee529d90");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f43b487-3f8e-426d-9a46-048c7d07f7f9",
                column: "ConcurrencyStamp",
                value: "4d2df8dd-8f24-4fb6-ac1f-4db646cfb66f");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "5f36c2f9-330a-492c-8ebf-65141782f2bb",
                column: "ConcurrencyStamp",
                value: "d4a7dab0-72ec-4f45-ac7f-8dfcd9158b67");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "6240e185-5c3a-410b-99d3-9767571fdf24",
                column: "ConcurrencyStamp",
                value: "7d9e2b52-7518-49c7-ad91-0625deb5448a");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "67320cb2-92a2-4de7-971b-7e9e80244f4b",
                column: "ConcurrencyStamp",
                value: "fa971a2e-ce68-4314-87ca-92ad7835d7c4");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c",
                column: "ConcurrencyStamp",
                value: "e7a60e90-aae7-4a36-b60b-0307df4a3418");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1477f6c-54ef-48d0-b24c-756b3a83b1a1",
                column: "ConcurrencyStamp",
                value: "7c01a114-3ede-469f-83b3-9fe7e457d82a");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e572b070-82bd-47f0-b486-cc1b644b2d9e",
                column: "ConcurrencyStamp",
                value: "5a55800b-8979-479f-b45e-896e7f852248");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5ac8272-7f9f-47c0-8e21-040fe3d242ed",
                column: "ConcurrencyStamp",
                value: "a9735c29-6424-4ed0-af63-fdba24f66130");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52",
                column: "ConcurrencyStamp",
                value: "10c20816-f415-4315-bc76-89af7265626f");

            migrationBuilder.UpdateData(
                table: "sec_AspNetRoles",
                keyColumn: "Id",
                keyValue: "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e",
                column: "ConcurrencyStamp",
                value: "50819a95-8ece-4c30-9adb-4e4292ae1302");
        }
    }
}
