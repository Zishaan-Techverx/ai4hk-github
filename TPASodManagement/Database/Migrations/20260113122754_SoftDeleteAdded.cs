using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Addresses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Addresses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "AddressTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "AddressTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "AreaTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "AreaTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Certificates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Certificates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "CertificateTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "CertificateTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Countries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Countries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Currencies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Currencies",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Customers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Farms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Farms",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Fields",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Fields",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Organizations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "People",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "People",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Permission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Products",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ProductCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "ProductCategories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Sales",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Sales",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "SaleLineItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "SaleLineItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "SaleTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "SaleTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Seedings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Seedings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "StateProvinces",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "StateProvinces",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Statuses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Statuses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "TagRanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "TagRanges",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Testimonials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Testimonials",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Wastes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Wastes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "WasteCertificate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "WasteCertificate",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "WasteReasons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "WasteReasons",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Websites",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "Websites",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "TagRanges");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "TagRanges");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "StateProvinces");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "StateProvinces");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Seedings");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Seedings");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "SaleTypes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "SaleTypes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "People");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "CertificateTypes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "CertificateTypes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AreaTypes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "AreaTypes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Addresses");
        }
    }
}
