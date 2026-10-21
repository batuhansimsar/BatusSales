# BatuSales — AI Business Operating System

BatuSales aims to help small businesses handle customer conversations, CRM, appointments
and eventually AI-assisted operations in one multi-tenant SaaS product.

Current repository state: FOUNDATION ONLY. It is not a production-ready MVP.
See docs/STATUS.md for implemented and missing features.

## Implemented API foundation

- .NET 10 minimal API, EF Core 10, PostgreSQL 17
- ASP.NET Core Identity register/login/refresh routes under /auth
- Authenticated organizations and membership-based ownership
- Tenant-authorized CRM contact creation, pagination and retrieval
- Server-side tenant filter, write guard, baseline rate limiting
- Development PostgreSQL via Docker and tenant-isolation test cases

## Getting started (local development)

Requirements: .NET 10 SDK, Docker with Compose.

1. Copy .env.example to .env and use only a local development password.
2. Run: docker compose up -d db
3. Set environment variables:
   ASPNETCORE_ENVIRONMENT=Development
   BATUSSALES_DEV_CREATE_DB=true
   ConnectionStrings__Postgres=Host=localhost;Port=5432;Database=batussales;Username=batussales;Password=local_development_password_only
4. Run: dotnet run --project src/BatusSales.Api --urls http://localhost:5080
5. Health check: GET http://localhost:5080/health
6. Run tests: dotnet test tests/BatusSales.Api.Tests

Note: ASP.NET Core Identity's /auth/login returns proprietary bearer + refresh tokens,
not JWTs, when useCookies=false. Keep tokens secret. For browser clients use the
appropriate cookie/CSRF model; do not store bearer tokens in localStorage.

Example API flow:
1. POST /auth/register with JSON {"email":"owner@example.test","password":"choose-a-strong-password"}
2. POST /auth/login?useCookies=false with the same JSON; capture accessToken.
3. POST /api/organizations (Authorization: Bearer TOKEN) {"name":"Demo Salon"}
4. GET /api/organizations to retrieve organization ID.
5. POST /api/organizations/{organizationId}/contacts (with bearer token)
   {"name":"Demo Contact","email":"client@example.test"}
6. GET /api/organizations/{organizationId}/contacts?page=1&pageSize=20

## Important security caveats

- BATUSSALES_DEV_CREATE_DB must NEVER be enabled in production.
  Local EnsureCreated initialization is temporary; production needs EF migrations.
- Email confirmation/verified ownership and outbound email are not configured yet.
- All CRM routes have membership authorization, but production still requires
  multi-tenant E2E testing, audit logging, TLS/proxy configuration and backups.
- The only team membership role created is Owner; full RBAC is not implemented.
- This is not a deployed product, and CI/build status must be checked in GitHub.

## Working agreement

Always inspect commits, branches, PRs, files and docs/STATUS.md before coding.
Avoid duplicate commits, prioritize missing P0 flows, and keep docs/TECH_RADAR.md
updated with source-backed technology decisions. See AGENTS.md.
