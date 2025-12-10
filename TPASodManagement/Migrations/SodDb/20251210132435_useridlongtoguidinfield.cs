using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Migrations.SodDb
{
    /// <inheritdoc />
    public partial class useridlongtoguidinfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_TpaUser_CreatedBy",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_CreatedByUserId",
                table: "Field");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Field",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "TpaUserUserId",
                table: "Field",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Field_TpaUserUserId",
                table: "Field",
                column: "TpaUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_TpaUser_TpaUserUserId",
                table: "Field",
                column: "TpaUserUserId",
                principalTable: "TpaUser",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_TpaUser_TpaUserUserId",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_TpaUserUserId",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "TpaUserUserId",
                table: "Field");

            migrationBuilder.AlterColumn<long>(
                name: "CreatedByUserId",
                table: "Field",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateIndex(
                name: "IX_Field_CreatedByUserId",
                table: "Field",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_TpaUser_CreatedBy",
                table: "Field",
                column: "CreatedByUserId",
                principalTable: "TpaUser",
                principalColumn: "UserId");
        }
    }
}
