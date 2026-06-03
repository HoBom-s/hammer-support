using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Support.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotificationTemplateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NotificationTemplate table is managed by hammer-internal (Flyway).
            // This empty migration updates the EF Core model snapshot only.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: table is managed by hammer-internal.
        }
    }
}
