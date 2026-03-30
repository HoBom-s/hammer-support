using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Support.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="NotificationTemplate"/>.
/// </summary>
internal sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.TemplateKey)
            .HasColumnName("template_key")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(t => t.TemplateKey).IsUnique();

        builder.Property(t => t.TitleTemplate)
            .HasColumnName("title_template")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(t => t.BodyTemplate)
            .HasColumnName("body_template")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(t => t.Channel)
            .HasColumnName("channel")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
