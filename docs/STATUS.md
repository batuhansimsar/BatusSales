# BatuSales — development status

Review date: 2026-10-21
Repository: batuhansimsar/BatusSales
Baseline main commit before this change: 08fef80b8e14d1627a0fbad5348f9da872548fa7
Current workstream: feat/foundation-auth-tenancy-crm-v1 (PR #8, not yet merged)

## Proposed in PR #8 (not shipped on main)
- .NET 10 API bootstrapping with PostgreSQL and rate limits.
- ASP.NET Core Identity registration/login/refresh endpoints.
- Authenticated organization onboarding and membership listing.
- Owner membership creation with new organization.
- Tenant membership endpoint guard (fail closed) for CRM.
- Contact create/list/get with explicit tenant ID on writes and EF query filters.
- SQLite tenant-isolation cases, including synchronous and both async save paths.
- Development-only database initialization and Docker PostgreSQL.
- **New independent increment:** PostgreSQL 17-backed tenant filter and write-guard
  regression test with two organizations and a no-tenant context. CI provisions an
  ephemeral PostgreSQL service and requires the test connection string.
  The test is skipped when BATUSSALES_TEST_POSTGRES is unset locally.
- Last validation MUST be checked against the latest branch commit in GitHub Actions.

## Competing foundation PRs
PRs #2, #4, #5, #6, #7, #8, #11 and #12 overlap heavily; #1 is an isolated Booking
domain proposal. Do not merge overlapping scaffolds without selecting one canonical
backend and porting only unique changes. Closed PR #3 was not merged.

## Not yet shipped / do not claim complete
- Database migrations and tested production deployment.
- Confirmed-email registration, recovery email sender, full Owner/Admin/Agent RBAC.
- Contact update/delete, deals, pipeline, lead scores.
- Dashboard UI and Next.js frontend.
- Web widget, inbox, AI agents, knowledge base, RAG.
- Booking, workflow, marketing, billing, audit logs, telemetry, backup/restore.
- Postgres RLS and authenticated API-level, cross-tenant end-to-end security tests.
- Subscription usage quotas and super-admin tools.

## Next action
1. Verify latest GitHub CI run and code review on PR #8.
2. Select one canonical foundation; do not merge #5/#8/#11/#12 together unchanged.
3. Generate and review initial EF migration; eliminate app EnsureCreated.
4. Implement verified identity/team permissions and authenticated HTTP integration tests.
5. Deliver web chat -> inbox -> CRM vertical slice, then booking.

## Quality / validation notes
The code was authored without a local .NET SDK. CI is the source of build and test
verification. Passing database model tests does not prove the HTTP membership guard,
RLS, production security, or tenant isolation of future modules.

## Proposed follow-up: Identity runtime hardening (stacked on PR #8)
- Discovered that MapIdentityApi<ApplicationUser> requires IEmailSender<ApplicationUser>
  at endpoint registration; the original foundation did not register the service.
- Added an explicitly Development-only, no-op email adapter that never prints
  confirmation links or password-reset tokens. Email verification and recovery
  remain nonfunctional in development until a real sender is implemented.
- Production/non-Development startup fails closed instead of silently allowing
  unverified accounts or pretending email messages were delivered.
- CI must boot the API with ephemeral PostgreSQL, exercise /auth/register and
  /auth/login, and confirm success. Compile-only CI was insufficient.
- This is a distinct follow-up to PostgreSQL tenant isolation tests (commit
  9526872), not another CRM foundation. Neither PR is merged into main.
