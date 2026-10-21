# Technology watch — 2026-10-21

Evaluated against this early, security-sensitive SaaS repository.

| Candidate | 2026 check | Decision |
|---|---|---|
| .NET 10 LTS | Supported to November 2028; patch 10.0.13 available | **Adopt** for stable backend |
| .NET 11 RC2 | Not GA as of 2026-10-21 | **Defer** pre-release dependency |
| Next.js 16.4 | Released 2026-10-06, lazy compilation and tooling improvements | **Next frontend milestone**, no UI yet |
| EF Core 10 named query filters | Useful for tenant and soft-delete rules | **Use tenant filter now**, assess further safeguards |
| pgvector 0.8.7 | 2026-10-01 bug fixes, HNSW is an option | **Defer** until sourced knowledge / RAG vertical slice |
| OpenTelemetry | Standard traces/metrics for request and tool spans | **Plan** after authenticated API integration tests |

## Decisions and cautions

- Do not add an LLM framework or vector database before ingest/query
  requirements are implemented and tested.
- Enforce tenant membership with server-controlled context. EF filters are not a
  replacement for DB privileges, DB row-level security or end-to-end tests.
- Use officially supported channel APIs, not scraping, for messaging integrations.
- Keep the next PR focused; resist premature microservice or queue deployment.

## Sources reviewed

- https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- https://dotnet.microsoft.com/en-us/download/dotnet
- https://nextjs.org/blog/next-16-4
- https://learn.microsoft.com/en-us/ef/core/querying/filters
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication
- https://github.com/pgvector/pgvector/blob/master/CHANGELOG.md
