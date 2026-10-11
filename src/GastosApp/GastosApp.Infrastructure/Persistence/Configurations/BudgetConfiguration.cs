using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets", t =>
        {
            t.HasCheckConstraint("CK_Budget_Owner",
                "(\"OwnerUserId\" IS NOT NULL AND \"GroupId\" IS NULL) OR " +
                "(\"OwnerUserId\" IS NULL AND \"GroupId\" IS NOT NULL)");

            t.HasCheckConstraint("CK_Budget_Amount",
                "\"LimitAmount\" > 0");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.LimitAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3);

        // Concurrencia optimista: xmin es la columna de sistema de PostgreSQL
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(b => b.OwnerUserId);
        builder.HasIndex(b => b.GroupId);
        builder.HasIndex(b => new { b.OwnerUserId, b.PeriodStart, b.PeriodEnd });
    }
}
