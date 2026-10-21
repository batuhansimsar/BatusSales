# BatuSales — AI Business Operating System

BatuSales is an early-stage, multi-tenant SaaS for AI-assisted business conversations, customer relationships, and bookings.

> **Status (2026-10-21): Foundation only. This is not a customer-ready product.** The marketing/developer console and service health endpoints are implemented. Authentication, tenant isolation, business APIs, AI agents, messaging, bookings, and billing are **not** implemented yet.

## Stack

- **API:** ASP.NET Core 10 (.NET 10 LTS), PostgreSQL readiness probe
- **Web:** Next.js 16 App Router, React 19, TypeScript
- **Database:** PostgreSQL 18 (local Docker development)
- **Architecture:** modular monolith; integrations are introduced as requirements mature

## Get started

1. Install Docker and Docker Compose.
2. Copy the example environment: `cp .env.example .env`.
3. Set a unique, non-default `DB_PASSWORD` in `.env`.
4. Run `docker compose up --build`.
5. Visit http://localhost:3000 for the web console.
6. Check http://localhost:5080/health/live and http://localhost:5080/health/ready.

Run the API separately with `dotnet run --project src/BatuSales.Api` (set `ConnectionStrings__BatuSales` for the readiness endpoint). Run the web UI with `cd apps/web && npm install && npm run dev`.

**Important:** `/health/live` checks the process. `/health/ready` tests PostgreSQL; it returns HTTP 503 until a working database connection is configured. The database is provisioned for development, but no business schema or migrations have been created yet. Do not enter real customer data.

## Repository layout

- `src/BatuSales.Api` — API and health checks
- `apps/web` — web developer console
- `tests/BatuSales.Api.Tests` — API smoke tests
- `docs/STATUS.md` — evidence-based completion ledger and next steps
- `.github/workflows/ci.yml` — build/test automation

## Engineering rules

1. **Read before writing:** inspect the current main branch, branches, commits, PRs, and tests.
2. **No duplicate changes:** if a feature already exists, extend/refactor it, do not recommit equivalent work.
3. **Incremental PRs:** build one tested vertical slice per change; avoid speculative infrastructure.
4. **Secure by default:** do not expose customer CRUD or AI actions until server-side identity, tenant authorization, and audit policies exist.
5. **Honest statuses:** planned != implemented; build passing != production safe.
6. **Dependency review:** verify stable/supported releases and advisories before upgrading.

See [docs/STATUS.md](docs/STATUS.md) for the active implementation checklist.
