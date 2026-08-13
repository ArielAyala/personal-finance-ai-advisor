using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Financial.Api.Endpoints;

/// <summary>
/// AI Assistant endpoints for chat and financial queries
/// </summary>
public static class AIAssistantEndpoints
{
    public static void MapAIAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI Assistant")
            .WithOpenApi();

        // Chat with AI assistant
        group.MapPost("/chat", async (
            [FromBody] ChatRequestDto request,
            [FromServices] IAIAssistantService aiService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // TODO: Get actual user ID from authentication
                var devUserId = new Guid("00000000-0000-0000-0000-000000000001");

                var response = await aiService.ChatAsync(devUserId, request, cancellationToken);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ChatWithAI")
        .WithSummary("Send a message to the AI financial advisor")
        .WithDescription(@"Chat with the AI assistant about your finances. 
The AI has access to your financial data and can answer questions about:
- Monthly spending and income
- Expense trends and patterns
- Net worth evolution
- Bills and obligations
- Account balances
- Savings rate

You can optionally provide conversation history to maintain context.")
        .Produces<ChatResponseDto>(200)
        .Produces(400);

        // Execute structured financial query
        group.MapPost("/query", async (
            [FromBody] string query,
            [FromServices] IAIAssistantService aiService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // TODO: Get actual user ID from authentication
                var devUserId = new Guid("00000000-0000-0000-0000-000000000001");

                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest(new { error = "Query cannot be empty" });
                }

                var result = await aiService.QueryFinancialDataAsync(devUserId, query, cancellationToken);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("QueryFinancialData")
        .WithSummary("Execute a structured financial query using AI")
        .WithDescription(@"Ask specific questions about your financial data.
The AI will use appropriate tools to fetch and analyze your data, 
then provide a structured response with sources.")
        .Produces<FinancialQueryResultDto>(200)
        .Produces(400);

        // Quick questions endpoint (common queries)
        group.MapGet("/quick-insights", async (
            [FromServices] IAIAssistantService aiService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // TODO: Get actual user ID from authentication
                var devUserId = new Guid("00000000-0000-0000-0000-000000000001");

                // Generate a quick financial summary
                var query = @"Provide a brief financial overview including:
1. Current month expenses vs income
2. Top spending category this month
3. Upcoming bills in the next 7 days
4. Latest net worth (if available)
Keep it concise and actionable.";

                var result = await aiService.QueryFinancialDataAsync(devUserId, query, cancellationToken);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetQuickInsights")
        .WithSummary("Get quick financial insights and summary")
        .WithDescription("Returns a concise overview of your current financial situation")
        .Produces<FinancialQueryResultDto>(200)
        .Produces(400);
    }
}
