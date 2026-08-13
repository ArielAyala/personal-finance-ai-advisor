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
        services.AddScoped<BillService>();
        services.AddScoped<FinancialSnapshotService>();
        services.AddScoped<ExchangeRateService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IGoalService, GoalService>();
        
        // Register AI services
        services.AddScoped<IAIAssistantService, AIAssistantService>();

        // Register import services
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<IFileParserService, CsvParserService>();
        services.AddScoped<IFileParserService, ExcelParserService>();

        // Register RAG and document services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IEmbeddingService, EmbeddingService>();
        services.AddScoped<ITextExtractionService, TextExtractionService>();
        services.AddScoped<IRagService, RagService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
