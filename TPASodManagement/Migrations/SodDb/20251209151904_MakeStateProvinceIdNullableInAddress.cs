using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Migrations.SodDb
{
    /// <inheritdoc />
    public partial class MakeStateProvinceIdNullableInAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Currency_CurrencyId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Product");

            migrationBuilder.AlterColumn<int>(
                name: "StateProvinceId",
                table: "Address",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Currency",
                table: "Product",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "CurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Currency",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "StateProvinceId",
                table: "Address",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Currency_CurrencyId",
                table: "Product",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "CurrencyId");
        }
    }
}
