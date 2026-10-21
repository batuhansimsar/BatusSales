# BatuSales technology radar

Reviewed: 2026-10-21. Prefer official sources and document why a dependency is added.

## Adopted now
- .NET 10 LTS and EF Core 10 for the initial API. .NET 10 support extends into November 2028.
  https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- ASP.NET Core Identity API endpoints for a compact registration/login/refresh baseline.
  Built-in opaque bearer tokens are NOT JWTs.
  https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0
- PostgreSQL 17 with Npgsql/EF Core 10 provider (10.0.3).
  https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/10.0.3
- ASP.NET Core rate-limiter middleware as a baseline; production needs proxy/network review.
  https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0

## Evaluate when implementing dependent features
- EF Core 10 named global query filters for combining soft-delete + tenant isolation.
  https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew
- PostgreSQL row-level security for defense-in-depth after role/session policy design.
  https://www.postgresql.org/docs/current/ddl-rowsecurity.html
- OpenTelemetry traces/metrics with no customer PII in spans.
  https://opentelemetry.io/docs/languages/dotnet/traces/getting-started-aspnetcore/
- pgvector for RAG once knowledge-base ingestion and relevance evaluations exist.
- Next.js/TypeScript frontend; pick and pin its stable version when frontend is added.

## Avoid for now
- .NET 11 RC for first paid customer: the stable supported .NET 10 LTS line is preferable.
- Early microservices, RabbitMQ, Redis, complex multi-agent orchestration without measured need.
- Unreviewed AI tool execution and auto-sending external messages.
