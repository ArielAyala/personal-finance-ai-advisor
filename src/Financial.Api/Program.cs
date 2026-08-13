using Financial.Api.Endpoints;
using Financial.Application;
using Financial.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Personal Finance AI Advisor API",
        Version = "v1",
        Description = "API for personal finance management with AI recommendations"
    });
});

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // Next.js dev server
                "https://localhost:3000")  // Next.js dev server (HTTPS)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Application services
builder.Services.AddApplication();

// Add Infrastructure services (Database, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Add Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Finance AI Advisor API v1");
        options.RoutePrefix = "swagger"; // Access Swagger UI at /swagger
    });
}

// CORS must be configured before any redirects
app.UseCors();

// Disable HTTPS redirection in development to avoid CORS issues with redirects
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Map API endpoints
app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
app.MapBillEndpoints();
app.MapImportEndpoints();
app.MapFinancialSnapshotEndpoints();
app.MapExchangeRateEndpoints();
app.MapAnalyticsEndpoints();

app.Run();
