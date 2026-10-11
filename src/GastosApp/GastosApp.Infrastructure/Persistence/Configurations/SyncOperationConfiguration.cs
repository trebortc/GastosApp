using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class SyncOperationConfiguration : IEntityTypeConfiguration<SyncOperation>
{
    public void Configure(EntityTypeBuilder<SyncOperation> builder)
    {
        builder.ToTable("SyncOperations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.OperationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => new { s.UserId, s.Status });
    }
}
