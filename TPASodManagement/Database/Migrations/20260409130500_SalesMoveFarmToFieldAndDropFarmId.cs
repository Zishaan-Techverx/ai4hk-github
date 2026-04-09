using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TpaSodManagement.Database;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409130500_SalesMoveFarmToFieldAndDropFarmId")]
    public partial class SalesMoveFarmToFieldAndDropFarmId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Backfill Sales.FieldId from Sales.FarmId using first active Field (lowest FieldId).
IF COL_LENGTH('Sales', 'FarmId') IS NOT NULL
BEGIN
    UPDATE s
    SET s.FieldId = fmatch.FieldId
    FROM Sales s
    OUTER APPLY (
        SELECT TOP (1) f.FieldId
        FROM Fields f
        WHERE f.FarmId = s.FarmId
          AND f.DeletedDate IS NULL
        ORDER BY f.FieldId
    ) fmatch
    WHERE s.FieldId IS NULL
      AND s.FarmId IS NOT NULL
      AND fmatch.FieldId IS NOT NULL;
END

-- 3) If some Sales farms have no Field, create one placeholder Field per farm.
IF EXISTS (
    SELECT 1
    FROM Sales s
    LEFT JOIN Fields f ON f.FarmId = s.FarmId AND f.DeletedDate IS NULL
    WHERE s.FieldId IS NULL
      AND s.FarmId IS NOT NULL
      AND f.FieldId IS NULL
)
BEGIN
    DECLARE @defaultAreaTypeId INT;
    DECLARE @defaultFieldTypeId BIGINT;

    SELECT TOP (1) @defaultAreaTypeId = AreaTypeId
    FROM AreaTypes
    WHERE DeletedDate IS NULL
    ORDER BY AreaTypeId;

    SELECT TOP (1) @defaultFieldTypeId = FieldTypeId
    FROM FieldType
    WHERE DeletedDate IS NULL
    ORDER BY FieldTypeId;

    IF @defaultAreaTypeId IS NULL OR @defaultFieldTypeId IS NULL
    BEGIN
        THROW 50011, 'Sales migration failed: cannot create placeholder fields (AreaType/FieldType missing).', 1;
    END

    ;WITH MissingFarm AS (
        SELECT DISTINCT s.FarmId
        FROM Sales s
        LEFT JOIN Fields f ON f.FarmId = s.FarmId AND f.DeletedDate IS NULL
        WHERE s.FieldId IS NULL
          AND s.FarmId IS NOT NULL
          AND f.FieldId IS NULL
    )
    INSERT INTO Fields
    (
        FarmId,
        FieldName,
        AreaAmount,
        AreaTypeId,
        FieldTypeId,
        IrrigationAvailable,
        CreatedDate,
        IsActive
    )
    SELECT
        mf.FarmId,
        CONCAT('Auto Field - Farm ', mf.FarmId),
        0,
        @defaultAreaTypeId,
        @defaultFieldTypeId,
        0,
        SYSUTCDATETIME(),
        1
    FROM MissingFarm mf;

    -- Re-run backfill after placeholder creation.
    UPDATE s
    SET s.FieldId = fmatch.FieldId
    FROM Sales s
    OUTER APPLY (
        SELECT TOP (1) f.FieldId
        FROM Fields f
        WHERE f.FarmId = s.FarmId
          AND f.DeletedDate IS NULL
        ORDER BY f.FieldId
    ) fmatch
    WHERE s.FieldId IS NULL
      AND s.FarmId IS NOT NULL
      AND fmatch.FieldId IS NOT NULL;
END

-- Hard-stop if any Sales row still has NULL FieldId.
IF EXISTS (SELECT 1 FROM Sales WHERE FieldId IS NULL)
BEGIN
    THROW 50010, 'Sales migration failed: some Sales rows could not be mapped to a FieldId.', 1;
END
");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Fields_FieldId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_FieldId",
                table: "Sales");

            migrationBuilder.AlterColumn<long>(
                name: "FieldId",
                table: "Sales",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_FieldId",
                table: "Sales",
                column: "FieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Fields_FieldId",
                table: "Sales",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "FieldId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty for forward-only data migration.
        }
    }
}
