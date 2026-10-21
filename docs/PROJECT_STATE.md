# BatuSales project state

Date: 2026-10-21

## Repository inspected before making changes

Repository: batuhansimsar/BatusSales
Default branch: main
Previously verified HEAD: 08fef80b8e14d1627a0fbad5348f9da872548fa7
Prior contents: README.md only.
Open pull requests and issues at inspection time: none.

## Initial foundation branch

Branch: feat/mvp-foundation-tenant-crm

Created:
- .NET 10 API using PostgreSQL and EF Core
- ASP.NET Core Identity, registration/login/refresh and development-only email sender
- Organization membership and owner bootstrap
- Tenant-scoped contact CRUD with soft deletion
- Audit event records for initial mutations
- Health checks, basic rate limits, tests, CI and Docker PostgreSQL

Validation status:
- The authoring environment has no local dotnet SDK.
- Initial PR CI (run 39654854078, commit b68ee895) passed restore, build and all six xUnit tests.
- New CI action versions are being checked on a follow-up PR run.
- Real PostgreSQL integration and E2E tests remain pending.

## Not yet completed (P0)

- Production email sender, verified accounts and password recovery
- EF Core migrations and real PostgreSQL integration tests
- CSRF-safe browser auth and team invitation/role updates
- SaaS onboarding frontend
- Web live chat widget, inbox and operator handoff
- Knowledge base / bounded AI agent and evaluation tests
- CRM lead and deal pipeline
- Booking and double-booking constraints
- Workflow automation and reliable job processing
- Billing subscriptions, usage limits and dashboard
- Deployment, backups, privacy controls and broader security review

## Rule for every future iteration

1. Reinspect main HEAD, relevant branches, PRs, and actual code before adding any change.
2. Compare against this list and previous commits. Do not repeat a feature or commit.
3. Check current stable packages, security advisories, platform changes and integration requirements.
4. Take the smallest valuable P0 milestone, develop it on a feature branch, add tests.
5. Update this file with exact SHA, tested status, completed work and next blockers.

## 2026-10-21 — Stacked P0 tenant contact DB guard (pending review)

- Base: PR #12, `feat/mvp-foundation-tenant-crm` @ `18a0ccfcf983`. This is **not merged to main**.
- Consolidation: adapted PR #8's context-scoped contact filter and save-time guard to PR #12's CRUD + audit design, without creating a third application scaffold.
- Additional defense: Contact.OrganizationId is an EF concurrency token; detached updates must match the tenant stored by the database.
- Coverage: SQLite relational tests for missing/denied membership, scoped reads, sync/async write denial and guessed-ID detached update.
- Pending: GitHub Actions results, PostgreSQL E2E + RLS review, EF migrations, verified-email production flow, role authorization. A query using IgnoreQueryFilters or ExecuteUpdate/Delete can bypass EF query/save guards, so authorization at endpoints and DB RLS defense remain required.
- Integration: stacked PR targets the PR #12 branch, **not main**; choose the canonical foundation before considering any merge.
