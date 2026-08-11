using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class SnapshotAccountConfiguration : IEntityTypeConfiguration<SnapshotAccount>
{
    public void Configure(EntityTypeBuilder<SnapshotAccount> builder)
    {
        builder.ToTable("snapshot_accounts");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.AccountNumberMasked)
            .HasMaxLength(100);

        builder.Property(sa => sa.OriginalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sa => sa.OriginalCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PYG");

        builder.Property(sa => sa.ExchangeRate)
            .HasPrecision(18, 6);

        builder.Property(sa => sa.ConvertedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sa => sa.BaseCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PYG");

        builder.HasOne(sa => sa.Snapshot)
            .WithMany(s => s.SnapshotAccounts)
            .HasForeignKey(sa => sa.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Account)
            .WithMany(a => a.SnapshotAccounts)
            .HasForeignKey(sa => sa.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => sa.SnapshotId);
        builder.HasIndex(sa => sa.AccountId);
    }
}
