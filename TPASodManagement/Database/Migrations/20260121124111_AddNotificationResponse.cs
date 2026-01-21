using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationResponse",
                columns: table => new
                {
                    NotificationResponseId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationResponse", x => x.NotificationResponseId);
                    table.ForeignKey(
                        name: "FK_NotificationResponse_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationResponse_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationResponse_NotificationId",
                table: "NotificationResponse",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationResponse_UserId",
                table: "NotificationResponse",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationResponse");
        }
    }
}
