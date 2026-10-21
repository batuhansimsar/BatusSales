# BatuSales vertical-slice roadmap

P0 means required for the first paid pilot; P1 and P2 follow validated customer demand.

| Order | Deliverable | Status |
| --- | --- | --- |
| 0 | .NET API, identity, PostgreSQL, organizations, tenant-scoped CRM contacts | Initial branch |
| 1 | Auth UI, onboarding, invitations and Owner/Admin/Agent authorization | Pending |
| 2 | Website chat widget, inbox, assignment and human handoff | Pending |
| 3 | AI knowledge base, scoped answers, safe tools, tests and model-cost budget | Pending |
| 4 | Staff/services/calendar and double-booking-safe appointments | Pending |
| 5 | Events, simple workflow and reliable outbox/retry | Pending |
| 6 | Deal pipeline, overview metrics, usage metering and subscriptions | Pending |
| 7 | Hardened production stack, security, compliance, E2E and paid pilot | Pending |

## Technology decision log — 2026-10-21

- .NET 10 is a currently supported LTS release. .NET 8 support ends in November 2026.
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 targets EF Core 10.
- ASP.NET Core Identity's token-based APIs use opaque access tokens, not standards-based JWT.
- Use modular monolith before adding RabbitMQ, Redis or distributed transactions.
- EF Core 10 supports named query filters; future tenant-aware DbContext design can strengthen default isolation.
- Next.js 16 receives security releases; use a patched stable version when the frontend starts, checking current advisories rather than freezing a vulnerable version.
- Prefer simple FAQ/catalog first; add pgvector/RAG after knowledge search evaluation warrants it.

Sources:
https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/10.0.3
https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0
https://learn.microsoft.com/en-us/ef/core/querying/filters
https://www.netlify.com/changelog/2026-10-14-nextjs-react-security-vulnerabilities/
