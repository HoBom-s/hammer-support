using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Support.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameChannelFcmToPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE notification_logs SET channel = 'Push' WHERE channel = 'Fcm'");
            migrationBuilder.Sql("UPDATE notification_templates SET channel = 'Push' WHERE channel = 'Fcm'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE notification_logs SET channel = 'Fcm' WHERE channel = 'Push'");
            migrationBuilder.Sql("UPDATE notification_templates SET channel = 'Fcm' WHERE channel = 'Push'");
        }
    }
}
