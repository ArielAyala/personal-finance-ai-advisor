using Financial.Application.DTOs;
using Financial.Application.Services;

namespace Financial.Api.Endpoints;

public static class ExchangeRateEndpoints
{
    public static void MapExchangeRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exchange-rates")
            .WithTags("Exchange Rates");

        group.MapGet("/", async (ExchangeRateService service) =>
        {
            var rates = await service.GetAllAsync();
            return Results.Ok(rates);
        })
        .WithName("GetExchangeRates")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, ExchangeRateService service) =>
        {
            var rate = await service.GetByIdAsync(id);
            return rate != null ? Results.Ok(rate) : Results.NotFound();
        })
        .WithName("GetExchangeRateById")
        .WithOpenApi();

        group.MapGet("/query", async (
            string sourceCurrency,
            string targetCurrency,
            DateTime? date,
            ExchangeRateService service) =>
        {
            ExchangeRateDto? rate;
            
            if (date.HasValue)
            {
                rate = await service.GetRateAsync(sourceCurrency, targetCurrency, date.Value);
            }
            else
            {
                rate = await service.GetLatestRateAsync(sourceCurrency, targetCurrency);
            }
            
            return rate != null ? Results.Ok(rate) : Results.NotFound();
        })
        .WithName("QueryExchangeRate")
        .WithOpenApi();

        group.MapGet("/convert", async (
            decimal amount,
            string sourceCurrency,
            string targetCurrency,
            DateTime? date,
            ExchangeRateService service) =>
        {
            var converted = await service.ConvertCurrencyAsync(amount, sourceCurrency, targetCurrency, date);
            
            if (!converted.HasValue)
            {
                return Results.NotFound(new { error = $"Exchange rate from {sourceCurrency} to {targetCurrency} not found" });
            }
            
            return Results.Ok(new
            {
                originalAmount = amount,
                sourceCurrency,
                convertedAmount = converted.Value,
                targetCurrency,
                date = date ?? DateTime.UtcNow.Date
            });
        })
        .WithName("ConvertCurrency")
        .WithOpenApi();

        group.MapGet("/range", async (
            DateTime startDate,
            DateTime endDate,
            ExchangeRateService service) =>
        {
            var rates = await service.GetByDateRangeAsync(startDate, endDate);
            return Results.Ok(rates);
        })
        .WithName("GetExchangeRatesByDateRange")
        .WithOpenApi();

        group.MapPost("/", async (CreateExchangeRateDto dto, ExchangeRateService service) =>
        {
            try
            {
                var rate = await service.CreateAsync(dto);
                return Results.Created($"/api/exchange-rates/{rate.Id}", rate);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateExchangeRate")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateExchangeRateDto dto, ExchangeRateService service) =>
        {
            try
            {
                var rate = await service.UpdateAsync(id, dto);
                return Results.Ok(rate);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateExchangeRate")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ExchangeRateService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteExchangeRate")
        .WithOpenApi();
    }
}
