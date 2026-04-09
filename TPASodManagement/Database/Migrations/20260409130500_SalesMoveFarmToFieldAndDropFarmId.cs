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
-- 1) Ensure Sales.FieldId exists.
IF COL_LENGTH('Sales', 'FieldId') IS NULL
BEGIN
    ALTER TABLE [Sales] ADD [FieldId] BIGINT NULL;
END

-- 2) Backfill Sales.FieldId from Sales.FarmId using first active Field (lowest FieldId).
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

-- 4) Hard-stop if any Sales row still has NULL FieldId.
IF EXISTS (SELECT 1 FROM Sales WHERE FieldId IS NULL)
BEGIN
    THROW 50010, 'Sales migration failed: some Sales rows could not be mapped to a FieldId.', 1;
END

-- 5) Drop FK/index dependencies on Sales.FieldId before altering nullability.
DECLARE @dropFieldFkSql NVARCHAR(MAX) = N'';
SELECT @dropFieldFkSql = @dropFieldFkSql +
    N'ALTER TABLE [' + sch.name + N'].[' + t.name + N'] DROP CONSTRAINT [' + fk.name + N'];'
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.schemas sch ON t.schema_id = sch.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = fkc.parent_column_id
WHERE t.name = 'Sales' AND c.name = 'FieldId';
IF (@dropFieldFkSql <> N'') EXEC sp_executesql @dropFieldFkSql;

DECLARE @dropFieldIdxSql NVARCHAR(MAX) = N'';
SELECT @dropFieldIdxSql = @dropFieldIdxSql +
    N'DROP INDEX [' + i.name + N'] ON [' + sch.name + N'].[' + t.name + N'];'
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas sch ON t.schema_id = sch.schema_id
WHERE t.name = 'Sales' AND c.name = 'FieldId' AND i.is_primary_key = 0 AND i.is_unique_constraint = 0;
IF (@dropFieldIdxSql <> N'') EXEC sp_executesql @dropFieldIdxSql;

-- 6) Make Sales.FieldId NOT NULL and recreate FK/index.
ALTER TABLE [Sales] ALTER COLUMN [FieldId] BIGINT NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sales_FieldId' AND object_id = OBJECT_ID('[Sales]'))
BEGIN
    CREATE INDEX [IX_Sales_FieldId] ON [Sales]([FieldId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sales_Fields_FieldId')
BEGIN
    ALTER TABLE [Sales] WITH CHECK
    ADD CONSTRAINT [FK_Sales_Fields_FieldId] FOREIGN KEY([FieldId]) REFERENCES [Fields]([FieldId]);
END

-- 7) Drop dependencies on Sales.FarmId then drop FarmId column.
IF COL_LENGTH('Sales', 'FarmId') IS NOT NULL
BEGIN
    DECLARE @dropFarmFkSql NVARCHAR(MAX) = N'';
    SELECT @dropFarmFkSql = @dropFarmFkSql +
        N'ALTER TABLE [' + sch.name + N'].[' + t.name + N'] DROP CONSTRAINT [' + fk.name + N'];'
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
    INNER JOIN sys.schemas sch ON t.schema_id = sch.schema_id
    INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = fkc.parent_column_id
    WHERE t.name = 'Sales' AND c.name = 'FarmId';
    IF (@dropFarmFkSql <> N'') EXEC sp_executesql @dropFarmFkSql;

    DECLARE @dropFarmIdxSql NVARCHAR(MAX) = N'';
    SELECT @dropFarmIdxSql = @dropFarmIdxSql +
        N'DROP INDEX [' + i.name + N'] ON [' + sch.name + N'].[' + t.name + N'];'
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.schemas sch ON t.schema_id = sch.schema_id
    WHERE t.name = 'Sales' AND c.name = 'FarmId' AND i.is_primary_key = 0 AND i.is_unique_constraint = 0;
    IF (@dropFarmIdxSql <> N'') EXEC sp_executesql @dropFarmIdxSql;

    ALTER TABLE [Sales] DROP COLUMN [FarmId];
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty for forward-only data migration.
        }
    }
}
