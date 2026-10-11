using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Icon)
            .HasMaxLength(50);

        builder.Property(c => c.Color)
            .HasMaxLength(20);

        // Indice parcial: nombre unico por usuario (NULL para categorias del sistema)
        builder.HasIndex(c => new { c.OwnerUserId, c.Name })
            .IsUnique()
            .HasFilter("\"OwnerUserId\" IS NOT NULL");

        // Filtra los eliminados en las consultas normales
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
