using Financial.Application.Interfaces;
using Financial.Infrastructure.Data;
using Financial.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Financial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FinancialDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.UseVector()
            ));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IFinancialSnapshotRepository, FinancialSnapshotRepository>();
        services.AddScoped<ISnapshotAccountRepository, SnapshotAccountRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentEmbeddingRepository, DocumentEmbeddingRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();

        return services;
    }
}
