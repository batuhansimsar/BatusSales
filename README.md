# BatusSales

AI Business Operating System — a multi-tenant SaaS for customer conversations, sales pipelines, appointments and responsible AI automation.

> Status: **foundation only**. This repository does not yet implement authentication, chat, CRM persistence, booking, AI agents or payments. Do not use it with real customer information.

## Repository layout

- `src/BatusSales.Api`: .NET 10 Minimal API host; liveness and system metadata endpoints.
- `src/BatusSales.Api/Tenancy`: strongly typed identifiers and organization slug validation. Not yet connected to authentication.
- `apps/web`: Next.js 16, TypeScript and Tailwind UI foundation with server-side API health proxy.
- `tests/BatusSales.Tests`: xUnit tests for core invariants.
- `docs/STATUS.md`: actual progress and next safe vertical slice.
- `docs/ARCHITECTURE.md`: architectural decisions and security boundaries.
- `docs/BACKLOG.md`: P0/P1/P2 roadmap.
- `docs/TECH_WATCH.md`: technology decisions and review process.

## Prerequisites

- .NET SDK 10
- Node.js 22 or later and npm
- No credentials are needed for the foundation endpoints.

## Run locally

Terminal 1:

    dotnet run --project src/BatusSales.Api --urls http://localhost:5080

Terminal 2:

    cd apps/web
    npm install
    npm run dev

Open http://localhost:3000. The UI proxies its backend health check via /api/health and expects BATUSSALES_API_URL (default http://localhost:5080).

Run backend unit tests:

    dotnet test tests/BatusSales.Tests/BatusSales.Tests.csproj

Run frontend checks:

    cd apps/web
    npm run typecheck
    npm run build

## Working rules (every development session)

1. Read the current default branch, recent commits, open PRs and docs/STATUS.md before editing.
2. Map proposed work to an unfinished P0/P1/P2 item; search the code for existing implementations.
3. Prefer the smallest cohesive vertical slice. Do not rewrite or duplicate already merged code.
4. Commit changes to a topic branch and open a PR; never automatically overwrite main.
5. Run available tests, verify CI, and report any test/environment gaps honestly.
6. Update docs/STATUS.md and docs/TECH_WATCH.md only when evidence warrants it.
7. Never mark business flows as complete without authentication, tenant isolation, negative security tests and persistence.

## Direction

Start with one small business segment (suggested: appointment-driven local services); interview customers before making broad integrations. Use a modular monolith before considering microservices. Refer to docs/BACKLOG.md for sequencing.
