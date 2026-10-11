using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ChangeSummary)
            .HasColumnType("jsonb");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6

        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => a.OccurredAt);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
