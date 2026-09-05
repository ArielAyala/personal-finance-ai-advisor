using Financial.Application.AI;
using Financial.Application.AI.Plugins;
using Financial.Application.Configuration;
using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Financial.Application.Services;

/// <summary>
/// AI Assistant service implementation using Semantic Kernel.
/// The chat-completion provider is selected at runtime via the AI:Provider configuration
/// key — no code changes required to switch between OpenAI and AzureOpenAI.
/// </summary>
public class AIAssistantService : IAIAssistantService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<AIAssistantService> _logger;

    public AIAssistantService(
        IConfiguration configuration,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAnalyticsService analyticsService,
        IFinancialSnapshotRepository snapshotRepository,
        IBillRepository billRepository,
        IGoalRepository goalRepository,
        IRagService ragService,
        ILogger<AIAssistantService> logger)
    {
        _logger = logger;

        var aiSettings = new AiSettings
        {
            Provider = configuration["AI:Provider"] ?? "OpenAI",
            OpenAI = new OpenAiProviderSettings
            {
                ApiKey = configuration["AI:OpenAI:ApiKey"] ?? string.Empty,
                ChatModelId = configuration["AI:OpenAI:ChatModelId"] ?? "gpt-4o-mini",
                EmbeddingModelId = configuration["AI:OpenAI:EmbeddingModelId"] ?? "text-embedding-3-small"
            },
            AzureOpenAI = new AzureOpenAiProviderSettings
            {
                Endpoint = configuration["AI:AzureOpenAI:Endpoint"] ?? string.Empty,
                ApiKey = configuration["AI:AzureOpenAI:ApiKey"] ?? string.Empty,
                ChatDeploymentName = configuration["AI:AzureOpenAI:ChatDeploymentName"] ?? string.Empty,
                EmbeddingDeploymentName = configuration["AI:AzureOpenAI:EmbeddingDeploymentName"] ?? string.Empty
            }
        };

        // Build kernel — provider selection is isolated in AIProviderKernelExtensions
        var kernelBuilder = Kernel.CreateBuilder()
            .AddChatCompletionProvider(aiSettings);

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

        // Register financial planning plugin for goals and recommendations
        var planningPlugin = new FinancialPlanningPlugin(
            goalRepository,
            analyticsService,
            transactionRepository,
            billRepository
        );
        kernelBuilder.Plugins.AddFromObject(planningPlugin, "FinancialPlanning");

        // Register recommendations plugin for advanced insights
        var recommendationsPlugin = new RecommendationsPlugin(
            analyticsService,
            transactionRepository,
            billRepository,
            goalRepository
        );
        kernelBuilder.Plugins.AddFromObject(recommendationsPlugin, "Recommendations");

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
            chatHistory.AddSystemMessage(@"You are a helpful personal finance AI advisor with advanced analytical capabilities. Your role is to:

1. Help users understand their financial situation comprehensively
2. Analyze spending patterns, trends, and financial health
3. Provide insights on savings, expenses, and investment opportunities
4. Track net worth evolution and financial progress
5. Monitor bills, obligations, and upcoming payments
6. Track financial goals with progress analysis and feasibility assessment
7. Answer questions about their financial data with accuracy
8. Search and retrieve information from their uploaded documents
9. Analyze affordability of purchases and major decisions
10. Recommend personalized savings allocation strategies
11. Assess goal feasibility and provide detailed planning advice
12. Identify specific savings opportunities and areas for improvement
13. Generate comprehensive action plans based on user's financial situation
14. Proactively suggest optimizations and financial health improvements

You have access to multiple tool categories:
- Financial Data Tools: Query transactions, accounts, analytics, bills, snapshots
- Document Search Tools: Semantic search through uploaded documents
- Financial Planning Tools: Goal tracking, affordability analysis, savings allocation
- Recommendations Tools: Health analysis, savings opportunities, action plans

When responding:
- Be concise, clear, and actionable
- Use specific numbers, percentages, and concrete examples
- Provide data-driven insights backed by actual financial data
- Format currency amounts appropriately (PYG or USD)
- Be supportive, constructive, and encouraging
- Prioritize recommendations by impact and urgency
- Explain your reasoning and the tools you used
- When analyzing complex scenarios, break down your analysis step-by-step
- If the user asks about documents or contracts, use document search tools
- When providing recommendations, always explain the 'why' behind each suggestion

The user's financial ecosystem includes:
- Transactions (income and expenses with detailed history)
- Accounts (bank accounts, credit cards, investments with current balances)
- Bills and recurring obligations with payment tracking
- Financial snapshots tracking net worth evolution over time
- Financial goals with progress tracking and deadline management
- Spending categories for detailed expense analysis
- Uploaded documents (statements, invoices, receipts, contracts, reports)

Your goal is to be a trusted financial advisor that helps users make better financial decisions through data-driven insights and personalized recommendations.");

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

            // Provider-agnostic execution settings.
            // FunctionChoiceBehavior lives on the base class (SK Abstractions).
            // Temperature / max_tokens are passed via ExtensionData; each connector's
            // FromExecutionSettings() reads them from there automatically.
            var executionSettings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.7,
                    ["max_tokens"] = 1000
                }
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

            var executionSettings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.3, // Lower temperature for more factual responses
                    ["max_tokens"] = 800
                }
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
