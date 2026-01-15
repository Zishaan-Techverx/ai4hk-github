using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    /// <inheritdoc />
    public partial class AuditedBaseEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Websites",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Websites",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Websites",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Wastes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Wastes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Wastes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "WasteReasons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "WasteReasons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "WasteReasons",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "WasteCertificate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "WasteCertificate",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "WasteCertificate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "WasteCertificate",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Testimonials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Testimonials",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Testimonials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Testimonials",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "TagRanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "TagRanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "TagRanges",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Statuses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Statuses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Statuses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "StateProvinces",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "StateProvinces",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "StateProvinces",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Seedings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Seedings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Seedings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "SaleTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "SaleTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "SaleTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Sales",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<long>(
                name: "UpdatedByUserId",
                table: "Sales",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Sales",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "SaleLineItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "SaleLineItems",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "SaleLineItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "SaleLineItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedDate",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreatedByUserId",
                table: "Products",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Products",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ProductCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ProductCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "ProductCategories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Permission",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Permission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "People",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "People",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "People",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Organizations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Organizations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreatedByUserId",
                table: "Fields",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Fields",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Fields",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Farms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Farms",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Farms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Farms",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Customers",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Currencies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Currencies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Currencies",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Countries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Countries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Countries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "CertificateTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "CertificateTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "CertificateTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Certificates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Certificates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Certificates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "AreaTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "AreaTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "AreaTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "AddressTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "AddressTypes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "AddressTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Addresses",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Addresses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Addresses",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "WasteCertificate");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TagRanges");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "TagRanges");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "TagRanges");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "StateProvinces");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "StateProvinces");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "StateProvinces");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Seedings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Seedings");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Seedings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SaleTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SaleTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SaleTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SaleLineItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CertificateTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "CertificateTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "CertificateTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AreaTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AreaTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AreaTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Addresses");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Websites",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Sales",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UpdatedByUserId",
                table: "Sales",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreatedByUserId",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "People",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Fields",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Customers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Addresses",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);
        }
    }
}
