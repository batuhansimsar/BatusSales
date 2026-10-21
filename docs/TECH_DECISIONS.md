# Technology choices — 2026-10-21

| Candidate | Decision | Reason |
| --- | --- | --- |
| .NET 10 LTS | Adopt | Supported to November 2028, better lifespan than .NET 8/.NET 9 ending November 2026. https://learn.microsoft.com/en-us/dotnet/core/releases-and-support |
| EF Core 10 + Npgsql 10 | Adopt | Compatible versions; start with relational tenant IDs, membership queries. https://www.npgsql.org/efcore/release-notes/10.0.html |
| PostgreSQL 18 | Development database | Native SQL and mature transaction semantics; CI integration tests and RLS must come before customers. https://www.postgresql.org/docs/current/sql-createpolicy.html |
| Next.js 16 App Router | Planned (not installed) | Supported modern frontend; add when a real UI slice is built. https://nextjs.org/docs/app |
| OpenAI Agents SDK | Defer | Strong tools, approvals, eval/tracing; TypeScript/Python SDK adds an extra runtime to a .NET-based MVP. Assess again for agent sprint. https://openai.github.io/openai-agents-js/ |
| Redis, RabbitMQ, MassTransit, pgvector | Defer | Do not introduce before there is a workload requiring them. |
| PostgreSQL RLS | Pre-pilot security gate | Membership + scoped queries are application-layer defense; RLS is a second layer and needs separate integration tests. |

## Migration caveat

Current Phase 0 code initializes schema with `EnsureCreatedAsync` in `Development` only. EF explicitly warns this is not migration-compatible. Before preserving real data, replace with generated migrations and recreate disposable dev databases as necessary. https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created

## Next evaluation

Before adding agent/tool-calling features, compare official .NET client support and model-agnostic options, security boundaries, structured outputs, costs, and trace/evaluation integrations. Record the decision before adoption.
