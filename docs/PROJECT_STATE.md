# BatuSales state — 2026-10-21

Main inspected before this increment: `d6cd72bcee006c455b3949a9ce59432cc48e5b8f`
Repository: https://github.com/batuhansimsar/BatusSales

## Verified merged foundation
- PR #12 merged as `a2b3bd5`: .NET 10 + PostgreSQL, Identity opaque tokens, organization/Owner membership, contact CRUD, Owner/Admin soft-delete, tenant query/write guards, audit, unit tests and authenticated PostgreSQL HTTP integration tests.
- PR #21 merged as `d6cd72b`: membership revocation denies previously issued access token. This test is not reimplemented.
- CI and dependency audit passed for `a2b3bd5` at initial inspection; independently check CI for the latest main commit.
- Competing foundation PRs (#2, #4–#8, #11) are alternatives, not independent merge candidates. Booking PR #1 has a separate domain-only proof, not a concurrency-safe persistence model.

## New change on feat/p0-tenant-faq-knowledge-20261021 (not merged)
- New tenant-scoped, customer-facing FAQ knowledge CRUD API: list, detail, create, update, soft-delete.
- Exact active membership required to read; database Owner/Admin role to write; other tenants respond 404.
- DbContext tenant query filter, guarded tracked writes, organization ownership concurrency token.
- Bounded question/answer, server-owned tenant and IDs; audit events omit content/PII.
- SQLite relational guard tests, validation/role tests and real PostgreSQL authenticated HTTP tests.
- This does NOT implement an agent, RAG, document ingestion, citations, chat, prompt-injection defenses or AI budgets.
- New FAQ table exists on **fresh development databases only** via `EnsureCreated`. Do not run against an existing database expecting schema upgrades. Reviewed EF migrations are still required before deployment.

## Highest-value pending P0 milestones
1. Production-ready migrations, verified email/secure recovery, secrets/TLS, secure browser auth and CSRF strategy.
2. Frontend onboarding, team invitations and role lifecycle.
3. Web chat widget, inbox, human handoff, FAQ-grounded AI agent with tool permissions/evaluation.
4. Booking with database-enforced collision safety, simple workflows/outbox, lead/deal pipeline, billing and quotas.
5. Backup/restore, consent/retention, tenant RLS assessment, E2E and monitoring.

## Technology review
Official EF Core supports DbContext-scoped query filters and concurrency tokens, but these are not a substitute for RLS/reviewed direct SQL. OWASP stresses server-verified tenant context. Use real PostgreSQL HTTP integration tests for authorization. Defer pgvector, Redis, RabbitMQ and agent frameworks until justified.
- https://learn.microsoft.com/en-us/ef/core/querying/filters
- https://learn.microsoft.com/en-us/ef/core/saving/concurrency
- https://learn.microsoft.com/en-us/aspnet/core/testing/integration-testing
- https://cheatsheetseries.owasp.org/cheatsheets/Multi_Tenant_Security_Cheat_Sheet.html

## Development protocol
Re-fetch main, PRs, branches, recent commits, CI, code and roadmap each session. Do not duplicate a merged change or competing feature PR. Small tested changes only; no unreviewed merge.
