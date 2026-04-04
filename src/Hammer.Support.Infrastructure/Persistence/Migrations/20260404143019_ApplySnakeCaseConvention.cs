using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Support.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplySnakeCaseConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_templates",
                table: "notification_templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_logs",
                table: "notification_logs");

            migrationBuilder.RenameIndex(
                name: "IX_notification_templates_template_key",
                table: "notification_templates",
                newName: "ix_notification_templates_template_key");

            migrationBuilder.RenameIndex(
                name: "IX_notification_logs_template_id",
                table: "notification_logs",
                newName: "ix_notification_logs_template_id");

            migrationBuilder.RenameIndex(
                name: "IX_notification_logs_recipient_token",
                table: "notification_logs",
                newName: "ix_notification_logs_recipient_token");

            migrationBuilder.RenameIndex(
                name: "IX_notification_logs_created_at",
                table: "notification_logs",
                newName: "ix_notification_logs_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification_templates",
                table: "notification_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification_logs",
                table: "notification_logs",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_notification_templates",
                table: "notification_templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notification_logs",
                table: "notification_logs");

            migrationBuilder.RenameIndex(
                name: "ix_notification_templates_template_key",
                table: "notification_templates",
                newName: "IX_notification_templates_template_key");

            migrationBuilder.RenameIndex(
                name: "ix_notification_logs_template_id",
                table: "notification_logs",
                newName: "IX_notification_logs_template_id");

            migrationBuilder.RenameIndex(
                name: "ix_notification_logs_recipient_token",
                table: "notification_logs",
                newName: "IX_notification_logs_recipient_token");

            migrationBuilder.RenameIndex(
                name: "ix_notification_logs_created_at",
                table: "notification_logs",
                newName: "IX_notification_logs_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_templates",
                table: "notification_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_logs",
                table: "notification_logs",
                column: "id");
        }
    }
}
