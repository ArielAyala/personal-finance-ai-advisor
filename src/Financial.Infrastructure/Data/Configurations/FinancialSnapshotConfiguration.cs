using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class FinancialSnapshotConfiguration : IEntityTypeConfiguration<FinancialSnapshot>
{
    public void Configure(EntityTypeBuilder<FinancialSnapshot> builder)
    {
        builder.ToTable("financial_snapshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Date)
            .IsRequired();

        builder.Property(s => s.BaseCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PYG");

        builder.Property(s => s.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.ExchangeRateDate);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.HasOne(s => s.User)
            .WithMany(u => u.FinancialSnapshots)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.Date);
        builder.HasIndex(s => s.Status);
    }
}
