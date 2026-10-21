# BatuSales

BatuSales is an AI Business Operating System for small businesses.

**Project status: initial technical foundation only. Not production ready.**

## Implemented on main (development baseline)

- .NET 10 / ASP.NET Core API with PostgreSQL (EF Core / Npgsql)
- ASP.NET Core Identity register, login and refresh endpoints
- Organization onboarding API and database-backed membership
- Tenant-scoped contacts: create, list, view, update and soft-delete
- Audit events, Owner/Admin-only tenant-scoped audit-history API, validation, health endpoints and basic rate limiting
- Unit tests and GitHub Actions build/test workflow

AI agents, web chat, unified inbox, appointment scheduling, billing, frontend, workflows and RAG are **not implemented yet**.

## Local development

Prerequisites: .NET 10 SDK and Docker Compose.

1. Copy .env.example to .env, then change POSTGRES_PASSWORD to a strong local password.
2. Run: docker compose up -d db
3. Set the environment variable for the same password:

   export ConnectionStrings__Default='Host=localhost;Port=5432;Database=batussales;Username=batussales;Password=YOUR_LOCAL_PASSWORD'

4. Start the API: dotnet run --project src/BatusSales.Api
5. Run tests: dotnet test tests/BatusSales.Api.Tests

The API defaults to http://localhost:5088 for the launch profile.

## API quickstart

- POST /auth/register — JSON body: email, password
- POST /auth/login?useCookies=false — JSON body: email, password
- POST /auth/refresh — refresh the opaque Identity access token
- POST /api/organizations — JSON body: name (requires authentication)
- GET /api/organizations — list your memberships
- GET /api/organizations/{organizationId}/contacts — list contacts
- POST /api/organizations/{organizationId}/contacts — JSON body: name, email, phone
- GET /api/organizations/{organizationId}/contacts/{contactId} — get contact
- PUT /api/organizations/{organizationId}/contacts/{contactId} — update contact
- DELETE /api/organizations/{organizationId}/contacts/{contactId} — soft-delete contact
- GET /api/organizations/{organizationId}/audit-events?page=1&pageSize=25 — tenant-scoped audit history (Owner/Admin only; max 100 rows/page)
- GET /health/live and GET /health/ready — health probes

For protected routes, send Authorization: Bearer ACCESS_TOKEN where ACCESS_TOKEN is returned by /auth/login. Built-in Identity bearer tokens are **opaque, not JWTs**.

Organization membership is checked server-side on every contacts endpoint. A user who has no membership sees 404 rather than another organization's data.

## Security and operational limits

**Do not deploy this as a real customer service yet.** Development mode creates an empty database via EF EnsureCreated. Production requires proper migrations, secure email verification and password-reset delivery, secrets, TLS, backup/restore and integration tests.

Only in Development: a no-op email sender is installed to allow experimenting with Identity. This disables usable email confirmation/recovery. Non-development startup is intentionally blocked until these flows are implemented.

Rate limiting is per instance in memory, not a production distributed limit. Browser clients should use HttpOnly, Secure and CSRF-protected cookies / BFF patterns instead of placing bearer tokens in localStorage.

See docs/PROJECT_STATE.md for exact code status and docs/ROADMAP.md for next milestones.
