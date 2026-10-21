# BatuSales — development status

Review date: 2026-10-21
Repository: batuhansimsar/BatusSales
Baseline main commit: 08fef80b8e14d1627a0fbad5348f9da872548fa7 (README only)
Canonical-backend candidate: PR #8 (`feat/foundation-auth-tenancy-crm-v1`).
This is **not** merged to main and is not a working/sellable MVP.

## Already proposed by PR #8 (do not duplicate)
- .NET 10 API, PostgreSQL, basic rate limits and health endpoint.
- ASP.NET Core Identity registration/login/refresh.
- Organization creation, Owner membership and membership-checked CRM endpoints.
- Contact create/list/detail with EF tenant filters and write guards.
- SQLite tenant isolation unit tests and API CI.

## This separate, stacked increment (new; pending CI)
- Real PostgreSQL-backed tenant isolation tests (not SQLite-only).
- Verify visibility by guessed contact ID, fail-closed queries without tenant,
  rejected cross-tenant writes and membership authorization denying foreign orgs.
- A dedicated CI job with disposable PostgreSQL 17 container, strict required
  `BATUSSALES_TEST_POSTGRES` variable and isolated test project.
- The tests use `EnsureCreated` **only for the disposable CI database**; the
  production migration gap remains open. No frontend, AI, booking or billing
  feature is claimed by this increment.

## Collaboration / consolidation blockers
- Multiple overlapping foundation drafts exist (#2, #3, #4, #5, #6, #7,
  #8, #11, #12). Do not merge several independent identity/CRM scaffolds.
- PR #8 is a leading candidate, **not an approved canonical architecture**.
  First review #12's audit/soft-delete additions and #4's OIDC approach;
  select one security model and one directory layout.
- Frontend work from #2 should be selectively integrated later rather than
  merging a second incompatible API skeleton.
- Booking PR #1 is domain-only and lacks database concurrency protection.

## Still missing (P0)
- Production identity email confirmation/recovery; tested EF migrations.
- Authenticated PostgreSQL endpoint E2E coverage and role-based permissions.
- Audit logging, backup/restore, tenant RLS assessment and data deletion policy.
- Next.js onboarding, web chat/inbox/handoff, grounded agent and knowledge base.
- Deal pipeline, transaction-safe bookings, workflow rules, billing and usage limits.
- Observability, TLS/secrets deployment review, AI security evaluations.

## Validation / next distinct steps
1. Require green API and PostgreSQL integration CI; local .NET SDK is unavailable
   in the authoring environment, so do not claim local compilation or execution.
2. Select and merge **one** reviewed backend foundation; this stacked test PR can
   then be retargeted to `main` without rewriting equivalent code.
3. Add reviewed EF migrations and negative auth E2E tests (revoked membership,
   ID guessing, role boundaries) before handling real customer information.
4. Bring in one compatible web frontend and deliver chat → inbox → human handoff.
5. Pilot one sector before starting P1/P2 technologies.

## Technology check (2026-10-21)
- .NET 10 LTS current October servicing patch is 10.0.13:
  https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- Database-backed booking overlap requires transactional exclusion constraints:
  https://www.postgresql.org/docs/current/rangetypes.html
- AI tool actions must respect least privilege and approval on sensitive effects:
  https://genai.owasp.org/llmrisk/llm062025-excessive-agency/
