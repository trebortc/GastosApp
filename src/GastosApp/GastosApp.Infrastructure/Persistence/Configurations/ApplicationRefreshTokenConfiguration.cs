using GastosApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosApp.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationRefreshTokenConfiguration : IEntityTypeConfiguration<ApplicationRefreshToken>
{
    public void Configure(EntityTypeBuilder<ApplicationRefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.DeviceInfo)
            .HasMaxLength(300);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
