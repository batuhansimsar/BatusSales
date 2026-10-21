# BatusSales — AI Business Operating System

AI-assisted customer communication, CRM, bookings and workflow automation for small businesses. This is an **early, non-production-ready** modular-monolith implementation.

## Current repository status (2026-10-21)

- **Implemented in the initial foundation PR:** ASP.NET Core 10 API scaffold, external OpenID Connect JWT validation, tenant-claim and database membership checks, tenant-filtered CRM contact read/create routes, local PostgreSQL schema, health probes, basic rate limiting, unit tests, CI.
- **Not implemented:** self-service authentication/onboarding, web frontend, web chat widget, AI agent, inbox, bookings, workflows, billing, production provisioning, tenant-aware end-to-end tests.
- See [docs/STATUS.md](docs/STATUS.md) for the acceptance checklist and next work.

## Prerequisites

- .NET 10 SDK (supported LTS)
- Docker Compose for a local PostgreSQL development database
- An OpenID Connect provider issuing **signed JWTs** with the configured audience, plus `sub` and `tenant_id` claims. There is **no** development bypass or checked-in signing key.

## Local setup

1. Copy `.env.example` to `.env` and replace the password with a unique local value.
2. Run `docker compose up -d db`. Initialization SQL applies only when the database volume is first created.
3. Set the API configuration through environment variables or .NET user secrets:
   - `ConnectionStrings__Postgres=Host=localhost;Port=5432;Database=batussales;Username=batussales;Password=<local-password>`
   - `Auth__Authority=https://<your-oidc-provider>`
   - `Auth__Audience=<configured-api-audience>`
4. Run `dotnet run --project src/BatusSales.Api`.
5. Call `GET /healthz` (liveness) or `GET /readyz` (database readiness).
6. To use CRM endpoints, create an organization UUID and a membership for your OIDC `sub` with SQL from the schema, ensuring your JWT contains the same `tenant_id` UUID. **Do not provision organizations or memberships from untrusted client requests.**

```sql
INSERT INTO organizations (id, name) VALUES ('11111111-1111-4111-8111-111111111111', 'Local demo');
INSERT INTO tenant_memberships (tenant_id, subject_id)
VALUES ('11111111-1111-4111-8111-111111111111', '<oidc-subject-id>');
```

## Routes

| Method | Path | Purpose |
|---|---|---|
| GET | `/healthz` | Public liveness |
| GET | `/readyz` | Public database readiness |
| GET | `/api/v1/contacts?limit=50` | Authenticated tenant contact list |
| GET | `/api/v1/contacts/{id}` | Authenticated tenant contact lookup |
| POST | `/api/v1/contacts` | Authenticated tenant contact creation (`{"name":"...","email":"..."}`) |

CRM routes require a validated JWT, a valid `tenant_id` claim and an active `tenant_memberships` row for its `sub`. Tenant IDs from URLs or request bodies are **never trusted**.

## Tests

`dotnet run --project tests/BatusSales.Api.Tests -c Release` runs xUnit v3 domain/auth tests; CI builds the API and runs these tests. A passing build is **not** proof of production-grade tenant isolation or OWASP LLM security. Before exposing customer data, add DB-backed cross-tenant integration tests, authorization negative cases, audit logs, backups and restore tests, consent/deletion workflows, security reviews, and a real provisioning flow.

## Decisions

- [ADR-0001](docs/ADR-0001.md): modular monolith, tenant model, and adoption criteria.
- Avoid introducing Redis, RabbitMQ, pgvector, external messaging providers, or LLM calls until an end-to-end P0 flow requires them.
