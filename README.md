# Personal Finance AI Advisor - Docker Setup

## Development Environment

This project uses Docker Compose to run PostgreSQL with pgvector extension for development.

### Prerequisites

- Docker Desktop installed
- .NET 9 SDK
- Node.js 18+ (for frontend)
- pnpm (recommended) or npm

### Start Development Database

```bash
docker-compose up -d
```

### Stop Development Database

```bash
docker-compose down
```

### Database Connection

```
Host: localhost
Port: 5432
Database: financial_db
User: financial_user
Password: financial_dev_password
```

### Connection String (for appsettings.Development.json)

```
Host=localhost;Port=5432;Database=financial_db;Username=financial_user;Password=financial_dev_password
```

### Verify pgvector Extension

After starting the container:

```bash
docker exec -it financial-postgres psql -U financial_user -d financial_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

## Backend Setup

```bash
# Restore packages
dotnet restore PersonalFinanceAIAdvisor.sln

# Build
dotnet build PersonalFinanceAIAdvisor.sln

# Run migrations (if not already applied)
dotnet ef database update --project src/Financial.Infrastructure --startup-project src/Financial.Api

# Run API
dotnet run --project src/Financial.Api
```

API will be available at:
- HTTP: http://localhost:5213
- HTTPS: https://localhost:7124
- OpenAPI: http://localhost:5213/openapi/v1.json (in development)

## Frontend Setup

```bash
cd src/frontend
pnpm install
pnpm dev
```

Frontend will be available at: http://localhost:3000

## Architecture

See:
- `init-spec-driven-development.md` - Product specification (8 phases)
- `Architecture-Specification-Document.md` - Technical architecture
- `AGENTS.md` - Project context for AI agents

## Phase 1 Status: ✅ COMPLETED

### What's Implemented:

**Backend:**
- ✅ Clean Architecture (Domain, Application, Infrastructure, Api)
- ✅ Entity models: User, Account, Category, Transaction, Bill, FinancialSnapshot, Goal, Document
- ✅ PostgreSQL + pgvector database
- ✅ EF Core migrations
- ✅ CRUD API endpoints:
  - `/api/accounts`
  - `/api/categories`
  - `/api/transactions`
- ✅ Repository pattern
- ✅ Dependency Injection

**Frontend:**
- ✅ Next.js 16 + TypeScript
- ✅ Tailwind CSS v4
- ✅ React Query for server state
- ✅ Axios API client
- ✅ Home page
- ✅ Accounts list page
- ✅ TypeScript types

**Infrastructure:**
- ✅ Docker Compose setup
- ✅ Multi-currency support (PYG, USD)
- ✅ CORS configured

### Next Steps:

**Phase 2: Import System**
- Excel/CSV file upload
- Transaction parser
- Duplicate detection
- Preview before import

**Phase 3: Financial Closing Module**
- Monthly snapshots (Draft/Closed)
- Net worth calculations
- Historical tracking

**Phase 4: Bills Module**
- Recurring bills
- Installment tracking
- Due date reminders

**Phase 5-8:** Analytics, AI Assistant, RAG, Advanced Agent

## Git Commit History

```bash
# View commits
git log --oneline

# Latest commit: Phase 1 - Core Financial System complete
```

## For Next Development Session

When continuing in a new chat/session, mention:
> "Continue with Phase 2 of Personal Finance AI Advisor.  
> Phase 1 is complete (see git log).  
> Refer to init-spec-driven-development.md and Architecture-Specification-Document.md for context."

The entire project structure is committed and ready for the next phase.
