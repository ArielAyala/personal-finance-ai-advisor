using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SourceCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.TargetCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Rate)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.SourceCurrency, e.TargetCurrency, e.Date })
            .IsUnique();
    }
}
