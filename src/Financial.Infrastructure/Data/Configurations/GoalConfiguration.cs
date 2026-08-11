using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.Property(g => g.TargetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.CurrentAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(g => g.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PYG");

        builder.Property(g => g.TargetDate);

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(g => g.User)
            .WithMany(u => u.Goals)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.UserId);
        builder.HasIndex(g => g.Status);
    }
}
