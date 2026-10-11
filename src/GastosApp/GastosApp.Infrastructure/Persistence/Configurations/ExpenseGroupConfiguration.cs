using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseGroupConfiguration : IEntityTypeConfiguration<ExpenseGroup>
{
    public void Configure(EntityTypeBuilder<ExpenseGroup> builder)
    {
        builder.ToTable("ExpenseGroups");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.Property(g => g.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("MXN");

        // Concurrencia optimista: xmin es la columna de sistema de PostgreSQL
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.CreatedByUserId);
    }
}
