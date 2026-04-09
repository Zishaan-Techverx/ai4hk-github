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
            migrationBuilder.AddColumn<int>(
                name: "TagStartNumber",
                table: "Seedings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TagEndNumber",
                table: "Seedings",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE [Seedings]
SET [TagStartNumber] = ISNULL([TagStartNumber], 0),
    [TagEndNumber] = ISNULL([TagEndNumber], 0);");

            migrationBuilder.AlterColumn<int>(
                name: "TagStartNumber",
                table: "Seedings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TagEndNumber",
                table: "Seedings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no down-drop/tightening in transitional phase.
        }
    }
}
