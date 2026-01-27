using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    /// <inheritdoc />
    public partial class FarmNameAddedInFarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FarmName",
                table: "Farms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmName",
                table: "Farms");
        }
    }
}
