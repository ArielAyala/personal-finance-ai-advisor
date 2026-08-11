using Financial.Application.Interfaces;
using Financial.Application.Services;
using Financial.Application.Services.Import;
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

        // Register import services
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<IFileParserService, CsvParserService>();
        services.AddScoped<IFileParserService, ExcelParserService>();

        return services;
    }
}
