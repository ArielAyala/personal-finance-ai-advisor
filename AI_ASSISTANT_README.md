# Phase 6 - AI Assistant v1 Configuration

## Overview

Phase 6 implements an AI-powered financial advisor using Semantic Kernel and OpenAI. The AI assistant can:

- Answer questions about your finances
- Analyze spending patterns and trends  
- Provide insights on savings and expenses
- Track net worth evolution
- Monitor bills and upcoming obligations
- Access real financial data through tools

## Configuration

### OpenAI API Key

The AI assistant requires an OpenAI API key. Configure it using **User Secrets** (recommended) or environment variables.

#### Option 1: User Secrets (Recommended for Development)

From the `src/Financial.Api` directory:

```bash
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
dotnet user-secrets set "OpenAI:ModelId" "gpt-4o-mini"
```

#### Option 2: Environment Variables

Set environment variables:

```bash
# Windows PowerShell
$env:OpenAI__ApiKey = "your-api-key-here"
$env:OpenAI__ModelId = "gpt-4o-mini"

# Linux/Mac
export OpenAI__ApiKey="your-api-key-here"
export OpenAI__ModelId="gpt-4o-mini"
```

#### Option 3: appsettings.Development.json (NOT RECOMMENDED - Don't commit this file)

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "ModelId": "gpt-4o-mini"
  }
}
```

**IMPORTANT: Never commit API keys to source control!**

### Supported Models

The implementation works with:
- `gpt-4o` - Most capable
- `gpt-4o-mini` - Fast and cost-effective (recommended)
- `gpt-4-turbo` - Good balance
- `gpt-3.5-turbo` - Cheapest option

## Features

### 1. Chat Interface (`POST /api/ai/chat`)

Natural conversation with the AI about your finances.

**Example:**
```json
{
  "message": "How much did I spend this month?",
  "history": [
    {
      "role": "user",
      "content": "Previous message",
      "timestamp": "2026-08-13T10:00:00Z"
    }
  ]
}
```

### 2. Structured Queries (`POST /api/ai/query`)

Execute specific financial queries with structured responses.

**Example:**
```json
"What were my total expenses for July 2026?"
```

### 3. Quick Insights (`GET /api/ai/quick-insights`)

Get an instant overview of your financial situation.

## Available AI Tools

The AI assistant has access to these financial data tools:

1. **get_monthly_summary** - Income, expenses, and savings for a specific month
2. **get_recent_transactions** - Latest transactions
3. **get_expense_trends** - Spending trends over time
4. **get_category_breakdown** - Spending by category
5. **get_net_worth** - Net worth evolution
6. **get_active_bills** - Upcoming bills and obligations
7. **get_debt_analysis** - Debt summary and payment tracking
8. **get_savings_rate** - Savings rate over time
9. **get_accounts_summary** - All account balances

The AI automatically selects and calls the appropriate tools based on your question.

## Testing

See `Financial.Api.http` for example requests:

- Simple questions about spending
- Savings rate analysis
- Net worth tracking
- Bill reminders
- Category analysis
- Financial planning questions

## Example Queries

**Spending Analysis:**
- "How much did I spend this month?"
- "What's my biggest expense category?"
- "Are my expenses increasing?"

**Savings & Income:**
- "Am I saving money?"
- "What's my savings rate?"
- "How much can I save this month?"

**Net Worth:**
- "What's my current net worth?"
- "How has my wealth changed this year?"
- "Show me my net worth evolution"

**Bills & Obligations:**
- "What bills do I have coming up?"
- "How much debt do I have?"
- "What are my monthly payments?"

**Financial Planning:**
- "Can I afford to buy [item]?"
- "How much should I save for [goal]?"
- "What expenses can I cut?"

## Architecture

```
User Question
     ↓
Chat/Query Endpoint
     ↓
AIAssistantService (Semantic Kernel)
     ↓
LLM (OpenAI) decides which tools to use
     ↓
FinancialDataPlugin tools
     ↓
Repository Layer → Database
     ↓
Structured JSON response
     ↓
LLM generates natural language answer
     ↓
User receives response
```

## Cost Considerations

- **gpt-4o-mini**: ~$0.15 per million input tokens, ~$0.60 per million output tokens
- Average query: 500-2000 tokens (input + output)
- Estimated cost: $0.001 - $0.002 per query

For production, consider:
- Rate limiting
- User quotas
- Caching frequent queries
- Using streaming responses for better UX

## Security

- API key stored in User Secrets (not in code)
- User ID required for all queries (data isolation)
- Financial data tools filter by userId automatically
- No PII sent to OpenAI (only aggregated financial data)

## Next Steps

**Phase 7 - RAG Integration:**
- Document ingestion (PDFs, Excel files)
- Embeddings with pgvector
- Financial knowledge base
- Context-aware responses

**Phase 8 - Advanced Agent:**
- Multi-step reasoning
- Financial planning
- Goal tracking
- Proactive recommendations
