using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Support.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    query = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    original_link = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    link = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    pub_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    collected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news_articles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_news_articles_original_link",
                table: "news_articles",
                column: "original_link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_news_articles_query_collected_at",
                table: "news_articles",
                columns: new[] { "query", "collected_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_articles");
        }
    }
}
