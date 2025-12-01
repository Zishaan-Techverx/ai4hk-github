using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Migrations.SodDb
{
    /// <inheritdoc />
    public partial class UpdatedRolePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "CanView",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "CanCreate",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "RolePermission");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RolePermission",
                type: "bit",
                nullable: false, 
                defaultValue: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                 name: "IsActive",
                 table: "RolePermission");

            migrationBuilder.AddColumn<bool>(
                name: "CanView",
                table: "RolePermission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreate",
                table: "RolePermission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                table: "RolePermission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                table: "RolePermission",
                type: "bit",
                nullable: false,
                defaultValue: false);

        }
    }
}
