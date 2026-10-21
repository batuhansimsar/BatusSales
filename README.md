# BatuSales — AI Business Operating System

BatuSales is a multi-tenant SaaS project for customer messaging, AI assistants,
CRM, appointment management and business automation.

> **Current state:** Early foundation, NOT a production-ready SaaS.
> The first vertical slice is authenticated organization onboarding and tenant-scoped
> CRM contacts. No chat widget, AI agent, inbox UI, booking or billing is implemented yet.

## Repository structure

- `apps/api` — ASP.NET Core 10 minimal API (modular monolith starting point).
- `db/init` — **local-only** PostgreSQL schema bootstrap.
- `tests/BatuSales.Tests` — tenant-scoping and write-guard unit tests.
- `docs/STATUS.md` — development status and next work.
- `docs/TECH_WATCH-2026-10-21.md` — technology research and decisions.
- `AGENTS.md` — rules for future development sessions.

## Requirements

- .NET 10 SDK
- Docker with Compose for local PostgreSQL
- An HTTPS OpenID Connect provider issuing signed JWT access tokens with a `sub`
  claim and the configured audience. No authentication bypass is provided.

## Local setup

1. Run `docker compose up -d db`. Port 5433 binds to localhost.
2. Set environment variables (the example is for **local development only**):
   ```bash
   export ConnectionStrings__BatuSales='Host=localhost;Port=5433;Database=batusales;Username=batusales;Password=local-development-only'
   export Auth__Authority='https://YOUR-OIDC-ISSUER'
   export Auth__Audience='batusales-api'
   ```
3. Start: `dotnet run --project apps/api/BatuSales.Api.csproj`.
4. Test: `dotnet test tests/BatuSales.Tests/BatuSales.Tests.csproj`.
5. Check `GET /health/live` and `GET /health/ready` on the bound API port.
   Only the health endpoints work without a JWT.

The API refuses to start when database/OIDC settings are missing, and requires
a valid HTTPS issuer in all environments. PostgreSQL schema bootstrap scripts run
only when the Docker volume is initially created; they are NOT a production
migration strategy.

## Authenticated API (first vertical slice)

Send `Authorization: Bearer <OIDC_ACCESS_TOKEN>` for all `/api` requests.

| Method | Route | Purpose |
|---|---|---|
| GET | /api/organizations | My organizations |
| POST | /api/organizations | Create business + owner membership |
| GET | /api/organizations/{id} | Read own organization |
| GET | /api/organizations/{id}/contacts | Paginated, tenant-filtered contacts |
| POST | /api/organizations/{id}/contacts | Create tenant-scoped contact |
| GET | /api/organizations/{id}/contacts/{contactId} | Read scoped contact |

Example requests:

```json
POST /api/organizations
{ "name": "Demo Beauty Salon" }
```

```json
POST /api/organizations/{id}/contacts
{ "fullName": "Sample Customer", "email": "sample@example.com", "phone": null, "notes": null }
```

The tenant ID is accepted as a route parameter **only after** checking the
authenticated user's membership in PostgreSQL. EF Core query filters block
out-of-tenant contact reads, and SaveChanges guards cross-tenant contact writes.
This is defense in depth, not a claim of complete tenant security: raw SQL,
elevated DB credentials, background processing, per-role permissions, PostgreSQL
RLS, rate limits and audit logging still require work.

## Roadmap

See [docs/STATUS.md](docs/STATUS.md). Prioritize first paid customer and P0
features before complex third-party messaging channels or enterprise options.

## Security notes

Never store real credentials in Git. Local Docker credentials are intentionally
not for production. Use a secrets manager, isolated DB roles, HTTPS termination,
migration discipline, authorization integration tests and monitoring before launch.

## Contribution workflow

Read [AGENTS.md](AGENTS.md) before making another commit or PR.
