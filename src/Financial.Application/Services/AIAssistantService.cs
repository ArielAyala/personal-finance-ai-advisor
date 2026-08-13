using Financial.Application.AI.Plugins;
using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Financial.Application.Services;

/// <summary>
/// AI Assistant service implementation using Semantic Kernel
/// </summary>
public class AIAssistantService : IAIAssistantService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<AIAssistantService> _logger;
    private readonly string _modelId;

    public AIAssistantService(
        IConfiguration configuration,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAnalyticsService analyticsService,
        IFinancialSnapshotRepository snapshotRepository,
        IBillRepository billRepository,
        IRagService ragService,
        ILogger<AIAssistantService> logger)
    {
        _logger = logger;

        // Get configuration
        var apiKey = configuration["OpenAI:ApiKey"] ?? 
                     throw new InvalidOperationException("OpenAI API key not configured");
        _modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";

        // Build kernel with OpenAI
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(_modelId, apiKey);

        // Register financial data plugin
        var financialPlugin = new FinancialDataPlugin(
            transactionRepository,
            accountRepository,
            analyticsService,
            snapshotRepository,
            billRepository
        );
        kernelBuilder.Plugins.AddFromObject(financialPlugin, "FinancialData");

        // Register RAG plugin for document search
        var ragPlugin = new RagPlugin(ragService);
        kernelBuilder.Plugins.AddFromObject(ragPlugin, "DocumentSearch");

        _kernel = kernelBuilder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<ChatResponseDto> ChatAsync(
        Guid userId,
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatHistory = new ChatHistory();

            // System prompt
            chatHistory.AddSystemMessage(@"You are a helpful personal finance AI advisor. Your role is to:

1. Help users understand their financial situation
2. Analyze spending patterns and trends
3. Provide insights on savings and expenses
4. Track net worth evolution
5. Monitor bills and upcoming obligations
6. Answer questions about their financial data
7. Search and retrieve information from their uploaded documents

You have access to financial data tools AND document search tools. Use them to provide accurate, data-driven answers.

When responding:
- Be concise and clear
- Use numbers and percentages when relevant
- Provide actionable insights
- Format currency amounts appropriately (PYG or USD)
- Be supportive and constructive
- If you need to use a tool, explain what you're checking
- If the user asks about documents, contracts, or specific information that might be in files, use the document search tools

The user's financial data includes:
- Transactions (income and expenses)
- Accounts (bank accounts, credit cards, investments)
- Bills and obligations
- Financial snapshots (net worth over time)
- Categories for organizing expenses
- Uploaded documents (bank statements, invoices, receipts, contracts, reports)");

            // Add conversation history if provided
            if (request.History != null)
            {
                foreach (var msg in request.History)
                {
                    if (msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                        chatHistory.AddUserMessage(msg.Content);
                    else if (msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
                        chatHistory.AddAssistantMessage(msg.Content);
                }
            }

            // Add current user message with userId context
            chatHistory.AddUserMessage($"[User ID: {userId}]\n{request.Message}");

            // Enable auto function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                MaxTokens = 1000
            };

            // Get response
            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cancellationToken
            );

            // Extract tools used (if any)
            var toolsUsed = response.Metadata?.ContainsKey("ToolCalls") == true
                ? string.Join(", ", ((IEnumerable<object>)response.Metadata["ToolCalls"]).Take(5))
                : null;

            return new ChatResponseDto(
                response.Content ?? "I apologize, but I couldn't generate a response.",
                toolsUsed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat for user {UserId}", userId);
            return new ChatResponseDto(
                "I apologize, but I encountered an error processing your request. Please try again.",
                null
            );
        }
    }

    public async Task<FinancialQueryResultDto> QueryFinancialDataAsync(
        Guid userId,
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(@"You are a financial data analyst. 
Your task is to answer specific financial queries using available tools.
Provide structured, data-driven answers with relevant metrics.
Always include the source of your data (which tables/tools you used).");

            chatHistory.AddUserMessage($"[User ID: {userId}]\n{query}");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.3, // Lower temperature for more factual responses
                MaxTokens = 800
            };

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cancellationToken
            );

            // Extract structured data if possible
            var data = new Dictionary<string, object>();
            var sources = new List<string>();

            // Try to extract tool calls from metadata
            if (response.Metadata?.ContainsKey("ToolCalls") == true)
            {
                var toolCalls = response.Metadata["ToolCalls"] as IEnumerable<object>;
                if (toolCalls != null)
                {
                    sources.AddRange(toolCalls.Select(tc => tc.ToString() ?? "").Where(s => !string.IsNullOrEmpty(s)));
                }
            }

            return new FinancialQueryResultDto(
                query,
                response.Content ?? "No answer generated",
                data.Count > 0 ? data : null,
                sources.Count > 0 ? sources : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in financial query for user {UserId}: {Query}", userId, query);
            return new FinancialQueryResultDto(
                query,
                $"Error processing query: {ex.Message}",
                null,
                null
            );
        }
    }
}
