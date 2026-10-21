# BatusSales — authoritative project state

Last reconciled: 2026-10-21 (Europe/Istanbul)
Repository: https://github.com/batuhansimsar/BatusSales
Milestone: **P0 development foundation, NOT a paid-pilot-ready MVP**.

## Canonical baseline on main

- **#12 merged to main:** merge commit `a2b3bd5935372311954e054266ebf25fa7e6265d`. Use the existing `src/BatusSales.Api` + `tests/` layout. Never scaffold another competing Identity/Tenancy/CRM backend.
- #12 incorporated the distinct reviewed ideas from **#15 (Owner/Admin CRM delete)** and **#16 (EF Core tenant query/write isolation and concurrency token)** as a combined commit `e885a734` and descendants. Do not cherry-pick those changes again.
- **#21 merged to main:** merge commit `d6cd72bcee006c455b3949a9ce59432cc48e5b8f`. PostgreSQL HTTP integration regression proves an already-issued token loses tenant contact access when membership is revoked. No duplicate test should be added on another foundation.
- Other foundation PRs (#4/#5/#6/#8/#11) are **alternatives**, not sequential migrations. #2/#7 contain alternative UI starter ideas to selectively evaluate; #1 holds booking-domain ideas, not an integrated booking service. #17/#20 relate to #8's Identity startup and are not changes to canonical main. #22 duplicates the revocation security invariant on #8's incompatible code base. Review unique ideas only; do not merge competing scaffolds.

## Present in main (development scope, not release guarantees)

- ASP.NET Core / .NET 10 API, EF Core + PostgreSQL development Docker Compose.
- ASP.NET Core Identity registration/login/refresh for **Development only**; no production email verification/provider.
- Authenticated organization creation, Owner membership and database-verified membership lookup on every tenant CRM request.
- Contacts create/list/read/update/soft-delete; server-side Owner/Admin only soft-delete, other members cannot delete.
- Contact tenant-scoped EF read filter, default-deny without verified tenant, guarded tracked writes, immutable tenant assignment, and organization ID concurrency token.
- Audit-event model, basic rate limiting, health/readiness endpoints.
- Unit tests, SQLite tenant-write regressions, real PostgreSQL HTTP cross-tenant tests and membership-revocation tests.
- GitHub Actions API build/test, PostgreSQL integration and NuGet direct+transitive dependency audit gates (moderate+).

## Verified checks and caveats

- #12 canonical head `7f2a7e0`: recorded successful CI and dependency-audit workflow runs; prior PostgreSQL and API jobs on #12 passed.
- #16 security checks passed before reconciliation; #21 head `98eae443` CI passed before merge into main.
- **Do not infer post-merge main workflow success from parent PR success.** Verify the current main SHA and its Actions results every session.
- There is no deployment, production-readiness certification, customer-data authorization, or business validation.

## P0 release blockers (order by dependency/risk)

1. EF Core **versioned migrations** and reviewed SQL/bundle, replacing development-only `EnsureCreated`; test fresh-install and schema upgrade on disposable PostgreSQL. Never run `EnsureCreated` on production customer data.
2. Real transactional email + confirmed-email login/recovery; secure browser session (cookie/BFF, CSRF, TLS/reverse-proxy settings); team invitations, owner/admin/agent authorization matrix and negative authenticated HTTP tests.
3. Database defense in depth: RLS/composite foreign keys review for all tenant-owned tables, direct SQL/bulk write boundaries, audit/access controls, deletion/retention/consent, backups and restore test, secrets and telemetry.
4. User-facing organization onboarding frontend and **web chat widget -> inbox -> human handoff -> grounded FAQ agent** vertical flow. Scope one target service sector; add tested AI budget/tool authorization and human confirmation.
5. CRM deals/pipeline, staff/services/booking with PostgreSQL overlap exclusion and concurrency tests, minimal workflow rules, dashboard, usage quotas and billing before accepting a paid pilot.
6. P1 integrations (WhatsApp/Instagram/calendar), RAG/pgvector, marketing and workflow builder remain postponed until P0 pilot evidence. P2 voice/SSO/multi-branch remains later.

## Relevant current official technology guidance

- .NET 10 LTS **10.0.13** security patch: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- EF Core 10 global tenant filters can be disabled by `IgnoreQueryFilters` and do not replace endpoint checks/database policies: https://learn.microsoft.com/en-us/ef/core/querying/filters
- EF Core recommends reviewed migration bundles or SQL scripts for production, with least-privilege deployment identity: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
- `EnsureCreated` is for disposable databases, not compatible with migration-based lifecycle: https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
- Identity browser clients should normally use cookie-backed sessions with CSRF protection: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization

## Required anti-duplication workflow each session

1. Inspect **live** main HEAD/tree, open and closed PRs, branches, commits, issue #14, this file and GitHub Actions before touching code.
2. Compare the feature against the user's P0/P1/P2 roadmap and classify: merged / proposed / missing / blocked.
3. If an existing PR or branch already handles it, improve or review that workstream; do not recreate its code, migration, test or commit under a different name.
4. Research official patch/security guidance relevant to the next smallest P0 slice; keep new infrastructure limited to demonstrated need.
5. Test against disposable PostgreSQL and CI; record exact new SHA and pass/fail, and do not claim production readiness without the release gates above.
