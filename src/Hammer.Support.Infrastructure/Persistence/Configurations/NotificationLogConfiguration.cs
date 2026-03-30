using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Support.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="NotificationLog"/>.
/// </summary>
internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.TemplateId)
            .HasColumnName("template_id")
            .IsRequired();

        builder.HasIndex(l => l.TemplateId);

        builder.Property(l => l.RecipientToken)
            .HasColumnName("recipient_token")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(l => l.Channel)
            .HasColumnName("channel")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(l => l.Body)
            .HasColumnName("body")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(l => l.CreatedAt);

        builder.Property(l => l.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2048);
    }
}
