using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Data;

public class FinancialDbContext : DbContext
{
    public FinancialDbContext(DbContextOptions<FinancialDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<FinancialSnapshot> FinancialSnapshots => Set<FinancialSnapshot>();
    public DbSet<SnapshotAccount> SnapshotAccounts => Set<SnapshotAccount>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentEmbedding> DocumentEmbeddings => Set<DocumentEmbedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
    }
}
