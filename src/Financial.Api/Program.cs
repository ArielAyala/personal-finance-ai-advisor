using Financial.Api.Endpoints;
using Financial.Application;
using Financial.Application.Configuration;
using Financial.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Financial.Application.DTOs.CreateAccountDto>();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
    };
});

builder.Services.AddAuthorization();

// Add Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Personal Finance AI Advisor API",
        Version = "v1",
        Description = "API for personal finance management with AI recommendations"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints
app.MapAuthEndpoints();
app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
app.MapBillEndpoints();
app.MapGoalEndpoints();
app.MapImportEndpoints();
app.MapFinancialSnapshotEndpoints();
app.MapExchangeRateEndpoints();
app.MapAnalyticsEndpoints();
app.MapAIAssistantEndpoints();
app.MapDocumentEndpoints();

app.Run();
