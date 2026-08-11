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
- Packages today: OpenAPI on Api; EF Core Design + **Npgsql.EntityFrameworkCore.PostgreSQL** on Infrastructure
- Api still has default weatherforecast sample; Domain/Application/Infrastructure are `Class1` stubs
- No tests, CI, Docker, or README yet
- No connection string / DbContext wired yet

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
- Planned domain cores: Account, Transaction, Category, Bill, FinancialSnapshot (+ SnapshotAccount), Goal, Document, ExchangeRate.
- Snapshot lifecycle: **Draft** (editable) → **Closed** (historical).
- Planned later (not in code): FluentValidation, JWT, pgvector (RAG), file import (Excel/PDF), AI agent tools, React Query / charts on frontend.
- Evolution: monolith CRUD → AI tools → RAG/pgvector → advanced agent. Build stage-appropriate; don’t invent full agent infra before the data model exists.

## Conventions

- Backend: nullable + implicit usings on all projects; wire DI in Api once Infrastructure has extension methods
- Frontend: App Router under `src/app/`; match existing template style until real UI lands
- Do not commit secrets; connection strings / API keys → user secrets or env, not checked-in appsettings
