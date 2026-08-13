# AGENTS.md

## What this is

Personal Finance AI Advisor — early full-stack scaffold. Product/architecture intent lives in:

- `Architecture-Specification-Document.md` (tech architecture)
- `init-spec-driven-development.md` (domain/spec)

Prefer executable code (`.csproj`, `package.json`, source) over docs when they disagree. Specs still define the target design.

## Layout

```
src/Financial.Api              ASP.NET Core host
src/Financial.Application      use cases
src/Financial.Domain           entities/rules
src/Financial.Infrastructure   EF Core + Npgsql
src/frontend                   Next.js app (not in the .sln)
```

Solution: `PersonalFinanceAIAdvisor.sln` (backend projects only). Frontend is separate under `src/frontend`.

## Backend stack

- .NET **9** (`net9.0`)
- Clean Architecture layers as above
- Packages today: 
  - **OpenAPI** + **Swagger** on Api
  - **FluentValidation.AspNetCore 11.3.1** on Api (auto-validation pipeline)
  - **EF Core Design** + **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4** + **Pgvector.EntityFrameworkCore 0.3.0** on Infrastructure
  - **Riok.Mapperly 4.3.1** on Application
  - **FluentValidation.DependencyInjectionExtensions 12.1.1** on Application (declarative validation)
  - **Microsoft.SemanticKernel 1.79.0** on Application (AI integration)
  - **iText7 9.0.0** on Application (PDF text extraction)
  - **EPPlus 7.5.2** on Application (Excel parsing & text extraction)
- Implemented modules:
  - **Phase 1**: Account, Transaction, Category CRUD with endpoints, services, repositories, EF configurations
  - **Phase 2**: Import System (CSV/Excel upload, parsing, duplicate detection)
  - **Phase 3**: Financial Closing Module (FinancialSnapshot, SnapshotAccount, net worth tracking)
  - **Phase 4**: Bills Module (recurring obligations, installments, payment tracking)
  - **Phase 5**: Analytics Module (monthly summaries, trends, savings rate, debt analysis)
  - **Phase 6**: AI Assistant v1 (Semantic Kernel + OpenAI integration with 9 financial data tools)
  - **Phase 7**: RAG Integration (Document upload/processing, pgvector semantic search, AI document-aware chat)
  - **Phase 8**: AI Agent Advanced (Goals module, financial planning tools, recommendations engine, multi-step reasoning)
- Entity→DTO mapping: **Mapperly** (source generator) with mappers in `Financial.Application/Mappers/`
- DTO→entity (Create/Update): **manual** in service layer (preserves business logic, validation, ownership checks)
- **Validation**: **FluentValidation** for all DTOs with declarative validators in `Financial.Application/Validators/`
  - Auto-validation pipeline configured in Program.cs
  - Validators for: Goals, Transactions, Bills, Accounts, FinancialSnapshots, SnapshotAccounts
  - Custom async validators for entity existence checks (Category, Account)
  - Common validation rules in BaseValidators.cs (currency validation PYG/USD)
  - Business logic validations (ownership, authorization) remain in service layer
- No tests, CI, Docker yet

### Dependency rules (enforce)

```
Api → Application, Infrastructure
Infrastructure → Application, Domain
Application → Domain
Domain → (nothing)
```

Do not reverse these. Domain must stay free of EF/ASP.NET packages.

### Backend commands

```bash
dotnet restore PersonalFinanceAIAdvisor.sln
dotnet build PersonalFinanceAIAdvisor.sln
dotnet run --project src/Financial.Api
```

- Dev URLs (`launchSettings.json`): `http://localhost:5213`, `https://localhost:7124`
- HTTP scratch: `src/Financial.Api/Financial.Api.http`
- When tests exist: `dotnet test` from solution root
- EF migrations: live in Infrastructure; use Api as startup project once DbContext exists

## Frontend stack (`src/frontend`)

Stock **create-next-app** template (App Router), still default marketing page — not product UI yet.

Verified from installed deps / source (root config files may be incomplete on disk):

- **Next.js 16.3** (App Router: `src/app/`)
- **React 19.2**
- **TypeScript** (~5.9)
- **Tailwind CSS v4** via `@import "tailwindcss"` in `src/app/globals.css` (not v3 `tailwind.config`)
- ESLint + `eslint-config-next` 16.3
- Package manager evidence: **pnpm** (`node_modules/.pnpm`)
- Entry files: `src/app/layout.tsx`, `src/app/page.tsx`, `src/app/globals.css`
- No API client, auth, or backend wiring yet; no React Query yet (planned in architecture doc)

### Frontend commands

Run from `src/frontend` (restore root manifests first if `package.json` / lockfile are missing):

```bash
pnpm install
pnpm dev      # default http://localhost:3000
pnpm build
pnpm lint
```

If only npm is available after a proper `package.json` exists: `npm install` / `npm run dev`. Prefer matching the lockfile (pnpm).

### Frontend gotchas

- Path is `src/frontend` (lowercase), not a .NET project — do not add it to the `.sln`
- Tailwind **v4**: theme tokens in CSS (`@theme inline`), not a classic `tailwind.config.js`
- Do not invent BFF routes in Next until API contracts exist; call `Financial.Api` once endpoints are real
- Keep frontend free of financial source-of-truth logic — that stays in PostgreSQL + backend

## Design constraints agents miss

- **Data first**: PostgreSQL is source of truth. AI reasons over tools/SQL; it must not own financial state.
- **Multi-currency from day one**: store original currency on accounts/transactions/obligations/snapshots. Primary currencies **PYG** and **USD**. Base reporting currency default **PYG** via historical `ExchangeRate` (user-entered).
- **Domain entities**: Account, Transaction, Category, Bill, FinancialSnapshot (+ SnapshotAccount), ExchangeRate, Document, DocumentEmbedding, Goal.
- Snapshot lifecycle: **Draft** (editable) → **Closed** (historical).
- **RAG (Phase 7 ✅)**: Document ingestion pipeline (upload → text extraction → chunking → OpenAI embeddings → pgvector storage), HNSW index for fast similarity search, RAG plugin for AI agent.
- **AI Architecture (Phase 8 ✅)**: Semantic Kernel orchestrates 4 plugin groups:
  1. **FinancialDataPlugin** (9 functions): transactions, accounts, bills, snapshots, analytics, debt analysis
  2. **RagPlugin** (2 functions): document semantic search, context retrieval
  3. **FinancialPlanningPlugin** (4 functions): goal tracking, affordability analysis, savings allocation, goal feasibility
  4. **RecommendationsPlugin** (3 functions): spending health analysis, savings opportunities, action plan generation
- **Goals Module (Phase 8 ✅)**: Full CRUD for financial goals with progress tracking, auto-completion detection, multi-currency support, repository/service/endpoints/DTOs
- **FluentValidation (Phase 9 ✅)**: Declarative validation layer with auto-validation pipeline. All DTOs validated before reaching service layer. Custom async validators for entity existence. Business logic validations remain in services.
- Planned later (not in code): JWT auth, advanced multi-agent orchestration, React Query / charts on frontend.
- Evolution: monolith CRUD → AI tools → RAG/pgvector (✅ complete) → advanced agent with planning/recommendations (✅ complete) → declarative validation (✅ complete). Build stage-appropriate.

## Conventions

- Backend: nullable + implicit usings on all projects; wire DI in Api once Infrastructure has extension methods
- Frontend: App Router under `src/app/`; match existing template style until real UI lands
- Do not commit secrets; connection strings / API keys → user secrets or env, not checked-in appsettings
