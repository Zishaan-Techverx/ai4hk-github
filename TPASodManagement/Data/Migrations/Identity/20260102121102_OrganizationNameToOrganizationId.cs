using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class OrganizationNameToOrganizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop OrganizationName column
            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "AspNetUsers");

            // Add OrganizationId column (nullable long)
            migrationBuilder.AddColumn<long>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            // Create index on OrganizationId for better query performance
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            // Add foreign key relationship for OrganizationId
            // Note: FarmId, PersonId, AddressId, WebsiteId foreign keys are managed by SodDbContext
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key for OrganizationId
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organizations",
                table: "AspNetUsers");

            // Drop index on OrganizationId
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers");

            // Drop OrganizationId column
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUsers");

            // Restore OrganizationName column
            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
