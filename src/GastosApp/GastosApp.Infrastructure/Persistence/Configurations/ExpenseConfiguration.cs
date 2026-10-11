using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses", t =>
        {
            // Restriccion CHECK: personal XOR grupo (nunca ambos, nunca ninguno)
            t.HasCheckConstraint("CK_Expense_Owner",
                "(\"PersonalOwnerUserId\" IS NOT NULL AND \"GroupId\" IS NULL) OR " +
                "(\"PersonalOwnerUserId\" IS NULL AND \"GroupId\" IS NOT NULL)");
        });

        builder.HasKey(e => e.Id);

        // Money como owned type (columnas embebidas en la misma tabla)
        builder.OwnsOne(e => e.Money, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        // Concurrencia optimista: xmin es la columna de sistema de PostgreSQL
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Eliminacion logica: excluye borrados en consultas normales
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.PersonalOwnerUserId);
        builder.HasIndex(e => e.GroupId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => new { e.PersonalOwnerUserId, e.OccurredAt });
        builder.HasIndex(e => new { e.GroupId, e.OccurredAt });
    }
}
