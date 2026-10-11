using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedOperationConfiguration : IEntityTypeConfiguration<ProcessedOperation>
{
    public void Configure(EntityTypeBuilder<ProcessedOperation> builder)
    {
        builder.ToTable("ProcessedOperations");

        builder.HasKey(p => p.Id);

        // El Id ES el OperationId enviado por el cliente — unicidad garantiza idempotencia
        builder.HasIndex(p => p.UserId);
    }
}
