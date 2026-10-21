# BatusSales — AI Business Operating System

Multi-tenant SaaS for AI-assisted customer messaging, CRM, scheduling and automation.

**Project status:** This is the first development foundation, **not a sellable MVP**. The public API currently exposes only `/health/live` and `/health/ready`. No registration, business API, payment flow or AI agent is live.

## Architecture (incremental)
- Backend: ASP.NET Core **.NET 10 LTS**, EF Core 10, PostgreSQL, modular monolith.
- Frontend (next milestone): Next.js + TypeScript + Tailwind.
- AI (later): provider-independent gateway, tool permissions, human approval, cost tracking.
- Redis, RabbitMQ, pgvector and multi-agent orchestration are **not** added before clear product need.

## Local development
1. Install the .NET 10 SDK (or Docker + Docker Compose).
2. Copy `.env.example` to `.env`; set a **unique** local PostgreSQL password. Do not commit `.env`.
3. Start PostgreSQL with `docker compose up -d postgres`.
4. Supply `ConnectionStrings__Postgres` in your shell and run `dotnet run --project src/BatusSales.Api`.
5. Call `GET /health/live` and `GET /health/ready`.
6. Run `dotnet test tests/BatusSales.Api.Tests/BatusSales.Api.Tests.csproj`.

**Important:** EF Core migrations have *not* been generated; persistable business endpoints are intentionally not exposed until authentication, tenant membership and initial migration have been implemented together.

## Repository working agreement
Before **every** increment: inspect main's HEAD SHA, open PRs, branches, features and CI; do not duplicate a commit or feature. Make a small branch and PR; refresh [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md) and the technology radar. Merge only after CI and review.

See [ROADMAP](docs/ROADMAP.md), [STATUS](docs/PROJECT_STATUS.md), [SECURITY](docs/SECURITY.md), and [TECH RADAR](docs/TECH_RADAR.md).
