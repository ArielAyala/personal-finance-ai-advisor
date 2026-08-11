using Financial.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Financial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<AccountService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();

        return services;
    }
}
