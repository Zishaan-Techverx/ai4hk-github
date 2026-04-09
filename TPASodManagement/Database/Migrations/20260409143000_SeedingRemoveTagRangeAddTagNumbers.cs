using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TpaSodManagement.Database;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409143000_SeedingRemoveTagRangeAddTagNumbers")]
    public partial class SeedingRemoveTagRangeAddTagNumbers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Seedings', 'TagStartNumber') IS NULL
BEGIN
    ALTER TABLE [Seedings] ADD [TagStartNumber] int NULL;
END;

IF COL_LENGTH('Seedings', 'TagEndNumber') IS NULL
BEGIN
    ALTER TABLE [Seedings] ADD [TagEndNumber] int NULL;
END;

EXEC sp_executesql N'
UPDATE [Seedings]
SET [TagStartNumber] = ISNULL([TagStartNumber], 0),
    [TagEndNumber] = ISNULL([TagEndNumber], 0);';

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Seedings' AND c.name = 'TagStartNumber' AND c.is_nullable = 1
)
BEGIN
    EXEC sp_executesql N'ALTER TABLE [Seedings] ALTER COLUMN [TagStartNumber] int NOT NULL;';
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Seedings' AND c.name = 'TagEndNumber' AND c.is_nullable = 1
)
BEGIN
    EXEC sp_executesql N'ALTER TABLE [Seedings] ALTER COLUMN [TagEndNumber] int NOT NULL;';
END;

IF COL_LENGTH('Seedings', 'TagRangeId') IS NOT NULL
BEGIN
    DECLARE @fkName nvarchar(200);
    SELECT TOP 1 @fkName = fk.name
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Seedings' AND c.name = 'TagRangeId';

    IF @fkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [Seedings] DROP CONSTRAINT [' + @fkName + ']');
    END;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Seedings_TagRangeId' AND object_id = OBJECT_ID('[Seedings]'))
    BEGIN
        DROP INDEX [IX_Seedings_TagRangeId] ON [Seedings];
    END;

    ALTER TABLE [Seedings] DROP COLUMN [TagRangeId];
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no down-drop/tightening in transitional phase.
        }
    }
}
