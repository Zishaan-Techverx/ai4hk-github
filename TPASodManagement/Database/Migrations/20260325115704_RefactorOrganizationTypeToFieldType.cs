using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrganizationTypeToFieldType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldType",
                columns: table => new
                {
                    FieldTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldType", x => x.FieldTypeId);
                });

            migrationBuilder.InsertData(
                table: "FieldType",
                columns: new[] { "FieldTypeName", "CreatedDate", "CreatedByUserId", "UpdatedDate", "UpdatedByUserId", "DeletedDate", "DeletedByUserId", "IsActive" },
                values: new object[,]
                {
                    { "RTF_Sod", new DateTimeOffset(new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc)), null, null, null, null, null, true },
                    { "RTF_HGT_Sod", new DateTimeOffset(new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc)), null, null, null, null, null, true },
                    { "HGT_Sod", new DateTimeOffset(new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc)), null, null, null, null, null, true }
                });

            migrationBuilder.AddColumn<long>(
                name: "FieldTypeId",
                table: "Fields",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE f
                SET f.FieldTypeId = ft.FieldTypeId
                FROM Fields f
                INNER JOIN Farms fm ON fm.FarmId = f.FarmId
                INNER JOIN Organizations o ON o.OrganizationId = fm.OrganizationId
                LEFT JOIN OrganizationType ot ON ot.OrganizationTypeId = o.OrganizationTypeId
                INNER JOIN FieldType ft ON ft.FieldTypeName =
                    CASE
                        WHEN COALESCE(ot.OrganizationTypeName, o.OrganizationTypeName) = 'RTF_Sod' THEN 'RTF_Sod'
                        WHEN COALESCE(ot.OrganizationTypeName, o.OrganizationTypeName) = 'RTF_HGT_Sod' THEN 'RTF_HGT_Sod'
                        WHEN COALESCE(ot.OrganizationTypeName, o.OrganizationTypeName) = 'HGT_Sod' THEN 'HGT_Sod'
                        ELSE 'RTF_Sod'
                    END
                WHERE f.FieldTypeId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE Fields
                SET FieldTypeId = (SELECT TOP 1 FieldTypeId FROM FieldType WHERE FieldTypeName = 'RTF_Sod')
                WHERE FieldTypeId IS NULL;
            ");

            migrationBuilder.AlterColumn<long>(
                name: "FieldTypeId",
                table: "Fields",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FieldTypeId",
                table: "Fields",
                column: "FieldTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fields_FieldType",
                table: "Fields",
                column: "FieldTypeId",
                principalTable: "FieldType",
                principalColumn: "FieldTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_OrganizationType",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_OrganizationTypeId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "OrganizationTypeId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "OrganizationTypeName",
                table: "Organizations");

            migrationBuilder.DropTable(
                name: "OrganizationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fields_FieldType",
                table: "Fields");

            migrationBuilder.DropTable(
                name: "FieldType");

            migrationBuilder.DropIndex(
                name: "IX_Fields_FieldTypeId",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "FieldTypeId",
                table: "Fields");

            migrationBuilder.AddColumn<long>(
                name: "OrganizationTypeId",
                table: "Organizations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationTypeName",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationType",
                columns: table => new
                {
                    OrganizationTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationType", x => x.OrganizationTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OrganizationTypeId",
                table: "Organizations",
                column: "OrganizationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_OrganizationType",
                table: "Organizations",
                column: "OrganizationTypeId",
                principalTable: "OrganizationType",
                principalColumn: "OrganizationTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
