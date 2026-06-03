using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Support.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="NewsArticle"/>.
/// </summary>
internal sealed class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("news_articles");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Query)
            .HasColumnName("query")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(a => a.OriginalLink)
            .HasColumnName("original_link")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(a => a.Link)
            .HasColumnName("link")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(a => a.PubDate)
            .HasColumnName("pub_date")
            .IsRequired();

        builder.Property(a => a.CollectedAt)
            .HasColumnName("collected_at")
            .IsRequired();

        builder.HasIndex(a => a.OriginalLink).IsUnique();
        builder.HasIndex(a => new { a.Query, a.CollectedAt });
    }
}
