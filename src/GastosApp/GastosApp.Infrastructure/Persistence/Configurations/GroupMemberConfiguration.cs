using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Un usuario no puede ser miembro duplicado del mismo grupo
        builder.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);
    }
}
