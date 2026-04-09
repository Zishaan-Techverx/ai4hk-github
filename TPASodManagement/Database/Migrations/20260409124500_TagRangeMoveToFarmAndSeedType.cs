using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TpaSodManagement.Database;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409124500_TagRangeMoveToFarmAndSeedType")]
    public partial class TagRangeMoveToFarmAndSeedType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('TagRanges', 'FarmId') IS NULL
BEGIN
    ALTER TABLE [TagRanges] ADD [FarmId] BIGINT NULL;
END

IF COL_LENGTH('TagRanges', 'SeedType') IS NULL
BEGIN
    ALTER TABLE [TagRanges] ADD [SeedType] INT NULL;
END

DECLARE @firstFarmId BIGINT;
SELECT TOP (1) @firstFarmId = [FarmId]
FROM [Farms]
WHERE [DeletedDate] IS NULL
ORDER BY [FarmId];

IF @firstFarmId IS NULL
BEGIN
    THROW 50020, 'TagRange migration failed: no farm record exists for FarmId backfill.', 1;
END

DECLARE @sql NVARCHAR(MAX);

SET @sql = N'UPDATE [TagRanges] SET [FarmId] = ' + CAST(@firstFarmId AS NVARCHAR(20)) + N' WHERE [FarmId] IS NULL;';
EXEC sp_executesql @sql;

EXEC sp_executesql N'UPDATE [TagRanges] SET [SeedType] = 1 WHERE [SeedType] IS NULL;';

EXEC sp_executesql N'ALTER TABLE [TagRanges] ALTER COLUMN [FarmId] BIGINT NOT NULL;';
EXEC sp_executesql N'ALTER TABLE [TagRanges] ALTER COLUMN [SeedType] INT NOT NULL;';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'IX_TagRanges_FarmId' AND [object_id] = OBJECT_ID('[TagRanges]'))
BEGIN
    CREATE INDEX [IX_TagRanges_FarmId] ON [TagRanges]([FarmId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = 'FK_TagRanges_Farms_FarmId')
BEGIN
    ALTER TABLE [TagRanges] WITH CHECK
    ADD CONSTRAINT [FK_TagRanges_Farms_FarmId] FOREIGN KEY([FarmId]) REFERENCES [Farms]([FarmId]);
END

IF COL_LENGTH('TagRanges', 'TagPrefix') IS NOT NULL
BEGIN
    ALTER TABLE [TagRanges] DROP COLUMN [TagPrefix];
END

IF COL_LENGTH('TagRanges', 'TagSuffix') IS NOT NULL
BEGIN
    ALTER TABLE [TagRanges] DROP COLUMN [TagSuffix];
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty (forward-only migration).
        }
    }
}
