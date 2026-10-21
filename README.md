# BatuSales — AI Business Operating System

A modular-monolith SaaS for business messaging, AI assistance, CRM, and scheduling.

> **Current state (2026-10-21): Phase 0 foundation, NOT a production-ready SaaS.** This first increment implements authentication, business onboarding, scoped CRM contacts, and a development database. It does **not** implement the AI agent, inbox, calendar, billing, or production migrations yet.

## Quick start (local development)

Requirements: Docker Engine and Docker Compose.

```bash
cp .env.example .env
# Change POSTGRES_PASSWORD to a local-only password.
docker compose up --build
curl http://localhost:8080/health/ready
```

Create a user:

```bash
curl -X POST http://localhost:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"owner@example.com","password":"A-long-demo-password-123!"}'
```

Log in to receive an ASP.NET Core Identity bearer token:

```bash
curl -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"owner@example.com","password":"A-long-demo-password-123!"}'
```

Use the returned `accessToken` in the `Authorization: Bearer <token>` header.

```bash
curl -X POST http://localhost:8080/api/organizations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"name":"Demo Salon"}'
```

Then use `/api/organizations/{organizationId}/contacts` to list and create CRM contacts. APIs for creating, viewing, and editing contacts are **membership checked and scoped by organization ID**.

## Development

- Backend: ASP.NET Core 10 / EF Core 10 / PostgreSQL, `src/BatusSales.Api`.
- Tests: `dotnet test tests/BatusSales.Api.Tests/BatusSales.Api.Tests.csproj`.
- CI: GitHub Actions `.github/workflows/ci.yml` runs .NET tests.
- Source of truth for roadmap progress: `docs/STATUS.md`.
- Agent/change review instructions: `AGENTS.md`.
- Technology evaluations: `docs/TECH_DECISIONS.md`.

### Important limitations

The database uses `EnsureCreated` **only under Development**, for disposable local prototyping. It cannot be used as a migration strategy; do not point this prototype at production or valuable data. Before pilot deployment, add and test actual EF Core migrations, verified email flows, audit coverage, integration tests, secrets/proxy/TLS configuration, PostgreSQL row-level security, retention policies, and backups. The API is deliberately not production-enabled without database initialization.

If the original repository's history or an existing PR already includes a planned task, inspect that work rather than creating it again.
