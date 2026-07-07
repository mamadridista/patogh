using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;

namespace Patogh.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedByIp).HasMaxLength(50);
        builder.Property(x => x.RevokedByIp).HasMaxLength(50);
        builder.Property(x => x.ReplacedByToken).HasMaxLength(500);

        // Fast lookup by token value
        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        // Cleanup job: find expired tokens by UserId
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_UserId_ExpiresAt");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user is deleted
    }
}