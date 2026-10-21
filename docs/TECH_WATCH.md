# Technology watch

Review date: 2026-10-21. Review again when choosing dependencies or implementing a new slice; do not upgrade merely because a version is newer.

- .NET 10 LTS: selected for the API host. Current official support lifecycle ends November 2028. Source: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- Next.js 16.x: selected for the frontend; 16.2 introduced a stable adapter API, which could help deploy beyond one hosting vendor. Source: https://nextjs.org/blog/nextjs-across-platforms
- EF Core 10 named query filters: candidate for tenant and soft-delete filtering. Filters are a defense-in-depth aid, not complete authorization. Source: https://learn.microsoft.com/en-us/ef/core/querying/filters
- ASP.NET Core rate limiting: built-in rate limiter used in the API host, not yet distributed; later consider reverse proxy and tenant/user quotas. Source: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit

## Revisit when feature demand justifies it

- PostgreSQL pgvector for sourced RAG; do not enable hallucination-prone retrieval before basic FAQ answers are tested.
- OpenTelemetry, durable queues and idempotent workers when real async actions are added.
- Identity provider trade-offs and supported billing providers for the pilot country.
- Next.js dependency lockfile and security scanning before shared deployments.

## Engineering policy

Every new candidate must document its capability, maintenance/security status, cost, migration impact, and simplest alternative. Keep unvalidated trends out of the hot path.
