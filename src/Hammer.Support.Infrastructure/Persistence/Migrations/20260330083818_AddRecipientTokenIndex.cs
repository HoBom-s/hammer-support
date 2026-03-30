using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Support.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipientTokenIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_recipient_token",
                table: "notification_logs",
                column: "recipient_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notification_logs_recipient_token",
                table: "notification_logs");
        }
    }
}
