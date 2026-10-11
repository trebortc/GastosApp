using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.ToTable("GroupInvitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InviteeEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.AssignedRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Solo puede haber una invitacion pendiente por email y grupo
        builder.HasIndex(i => new { i.GroupId, i.InviteeEmail })
            .IsUnique()
            .HasFilter("\"Status\" = 'Pending'");

        builder.HasIndex(i => i.InviteeEmail);
    }
}
