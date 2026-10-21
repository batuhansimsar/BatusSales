# BatuSales project state

Date: 2026-10-21
Repository: batuhansimsar/BatusSales
Default branch: main (README-only baseline 08fef80b8e14d1627a0fbad5348f9da872548fa7)

## Coordination / duplicate prevention

At the latest inspection, ten draft PRs (#1-#8, #11, #12) were open.
PRs #2-#8, #11 and #12 contain competing foundations with different directory
layouts, authentication methods and tenancy patterns. They are alternatives,
**not** ten independent components to merge. PR #1 is a separate booking-domain
preflight without database-level concurrency guarantees. Do not merge multiple
foundation PRs until the architecture and security checks are reconciled.

This document tracks branch `feat/mvp-foundation-tenant-crm` (PR #12) as
a **candidate**, not a selected or merged canonical foundation.

## Implemented in PR #12 (not main)

- .NET 10 API using PostgreSQL, EF Core and ASP.NET Core Identity opaque tokens.
- Development-only registration/login/refresh and disposable DB bootstrap.
- Organization creation with Owner membership and membership-checked contact endpoints.
- Contact create/list/read/update/soft-delete with explicit organization predicates.
- Audit events, liveness/readiness probes, baseline rate limiting and unit tests.
- Docker Compose and GitHub Actions CI.

## Increment — PostgreSQL HTTP isolation tests (2026-10-21)

New code on this *existing branch* (no duplicate foundation or competing PR):
- `tests/BatusSales.Api.IntegrationTests/` — separate integration test project.
- Authenticate two separate users through actual HTTP register/login, create
  separate organizations, and test member vs non-member operations against
  an ephemeral PostgreSQL service: list, read, create, update, delete.
- Verify anonymous access is denied; soft-deleted rows disappear from the API
  but remain soft-deleted in relational storage; rejected update does not
  change stored records or create an update audit event.
- `.github/workflows/ci.yml` — a PostgreSQL 17 service CI job runs this
  test project; the existing unit-test job remains intact.
- Integration suite refuses to run without an explicitly named
  `batussales_test` database and `BATUSSALES_TEST_POSTGRES`.
- First integration test commits: 01ce545, 3afd4b1; CI wiring: 4f66623.

### Validation and limitations

- Previous six unit tests passed in GitHub Actions (run 39655088247).
- **New integration tests are not considered passing until the GitHub Actions
  postgres-integration job completes successfully.** No local .NET SDK.
- This tests HTTP membership gates with real PostgreSQL; it does **not**
  establish DB row-level security, production token security, or cross-tenant
  protection against direct unsafe SQL, and does not replace independent review.
- Existing dev-only `EnsureCreated` is not a real migration workflow.
- Do not deploy to paying tenants or merge competing backend PRs without review.

## Not yet completed (P0)

- Review/select one canonical foundation and safely port only unique work.
- Versioned EF migrations, safe production identity/email confirmation,
  authorization negative cases, defense-in-depth RLS assessment.
- Secure browser authentication, CSRF controls and invitation/role workflows.
- Product onboarding frontend and web chat widget to unified inbox.
- Grounded AI agent with least-privilege tool policy, audit and human handoff.
- CRM deal pipeline; booking with database-level overlap exclusion;
  reliable workflow execution; dashboard; quotas/billing.
- TLS/secret hardening, backup/restore tests, deletion and consent workflows,
  secure logs, monitoring and staged deployment.

## Technology review (2026-10-21)

- .NET 10 LTS + EF Core 10 remain appropriate for the proposed backend.
- Official Microsoft ASP.NET Core testing guidance supports
  `WebApplicationFactory` for authenticated HTTP integration tests.
- GitHub Actions officially supports disposable PostgreSQL service containers.
- OWASP recommends enforcing AI tool authorization in application code,
  minimizing privileges and requiring human approval for high-impact actions.
- Do not add Redis, RabbitMQ, pgvector, multi-agent frameworks or channel
  integrations until an actual P0 vertical slice needs them.

References:
- https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- https://learn.microsoft.com/en-us/aspnet/core/testing/integration-testing
- https://docs.github.com/en/actions/tutorials/use-containerized-services/create-postgresql-service-containers
- https://cheatsheetseries.owasp.org/cheatsheets/LLM_Prompt_Injection_Prevention_Cheat_Sheet.html

## Iteration protocol

1. Inspect `main` HEAD, recent commits, open PRs, changed files and status.
2. Compare actual code/tests (not claims) with the P0/P1/P2 roadmap.
3. Avoid committing the same feature on another branch.
4. Verify relevant current platform/security guidance before new dependencies.
5. Improve one scoped increment; add tests and preserve production safety.
6. Record exact commit SHA, CI outcome, blockers and next acceptance criteria.

## Increment — contact DB query/write guards (stacked PR #16)

- Base PR #12 was refreshed with PostgreSQL HTTP tenant-isolation tests before this merge.
- Adapted #8's tenant-scoped contact query and tracked-write protections to #12's existing Contact CRUD.
- A successful current membership check activates a tenant in the scoped EF context; a denied check clears it.
- `Contact.OrganizationId` is an EF concurrency token, so detached updates/soft-delete writes include original tenant ownership in the SQL predicate.
- Added SQLite relational regression tests for no-membership read denial, cross-tenant sync/async saves, and forged cross-tenant contact IDs.
- PR #16 first commit `ae137a2` passed the API and PostgreSQL integration jobs: https://github.com/batuhansimsar/BatusSales/actions/runs/39655874518
- **This stacked contribution is not merged to main**, and PostgreSQL RLS / unsafe bulk SQL / migrations / production auth remain open gates.
- Refreshed against PR #12 base commit `1bc1d2ce588d` without force push or rewriting existing commits.
