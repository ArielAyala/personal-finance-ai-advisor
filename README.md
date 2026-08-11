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
cd src/Financial.Api
dotnet restore
dotnet ef database update --project ../Financial.Infrastructure
dotnet run
```

## Frontend Setup

```bash
cd src/frontend
pnpm install
pnpm dev
```

## Architecture

See:
- `init-spec-driven-development.md` - Product specification
- `Architecture-Specification-Document.md` - Technical architecture
