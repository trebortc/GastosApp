using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(p => p.UserId);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(p => p.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("MXN");
    }
}
