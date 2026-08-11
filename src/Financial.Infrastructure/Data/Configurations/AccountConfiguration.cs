using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PYG");

        builder.Property(a => a.AccountNumber)
            .HasMaxLength(100);

        builder.Property(a => a.Institution)
            .HasMaxLength(200);

        builder.Property(a => a.CurrentBalance)
            .HasPrecision(18, 2);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}
