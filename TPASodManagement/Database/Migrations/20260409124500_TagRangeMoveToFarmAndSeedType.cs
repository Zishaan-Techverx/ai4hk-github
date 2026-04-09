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
            migrationBuilder.AddColumn<long>(
                name: "FarmId",
                table: "TagRanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeedType",
                table: "TagRanges",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
DECLARE @firstFarmId BIGINT;
SELECT TOP (1) @firstFarmId = [FarmId]
FROM [Farms]
WHERE [DeletedDate] IS NULL
ORDER BY [FarmId];

IF @firstFarmId IS NULL
BEGIN
    THROW 50020, 'TagRange migration failed: no farm record exists for FarmId backfill.', 1;
END;

UPDATE [TagRanges] SET [FarmId] = @firstFarmId WHERE [FarmId] IS NULL;
UPDATE [TagRanges] SET [SeedType] = 1 WHERE [SeedType] IS NULL;");

            migrationBuilder.AlterColumn<long>(
                name: "FarmId",
                table: "TagRanges",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeedType",
                table: "TagRanges",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagRanges_FarmId",
                table: "TagRanges",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_TagRanges_Farms_FarmId",
                table: "TagRanges",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "FarmId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty (forward-only migration).
        }
    }
}
