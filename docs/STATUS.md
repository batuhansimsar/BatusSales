# Delivery status

Snapshot date: 2026-10-21
Base branch reviewed: main at 08fef80b8e14d1627a0fbad5348f9da872548fa7
Prior contents: README.md only; no PRs or issues.

## Actual state

- [x] Repository audit before coding
- [x] API scaffold (.NET 10) with liveness and system metadata endpoints
- [x] Frontend scaffold (Next.js 16 + TypeScript + Tailwind)
- [x] Server-to-server health proxy in the web app
- [x] First tenant identifier and organization slug validation rules
- [x] Unit tests added for value objects
- [x] Pull-request CI configured
- [ ] CI verified green on the PR (pending GitHub Actions run)
- [ ] Authentication, account recovery and verified email
- [ ] Organization persistence, authenticated memberships and tenant isolation enforcement
- [ ] Web widget, inbox and human handoff
- [ ] AI agents, knowledge base and safe tool execution
- [ ] CRM, bookings and calendar
- [ ] Workflow engine
- [ ] Subscription billing and metered usage
- [ ] Production observability, backups and security verification

IMPORTANT: Value object validation is not tenant isolation. No tenant-bound data endpoints should be deployed until authenticated membership checks plus database filters and negative tests are implemented.

## Next development slice (P0)

Identity + Organizations + Tenancy, backed by PostgreSQL:

1. Select a server-side identity mechanism with verified email and password-reset flows.
2. Model User, Organization, Membership, Role, TenantScoped entities.
3. Resolve active tenant only from the authenticated principal plus database membership (never trust arbitrary X-Tenant-ID header).
4. Use EF Core query filters and explicit write-side tenant checks; add migration and unique indexes.
5. Add integration tests for cross-tenant reads, writes, role boundaries and revoked memberships.
6. Introduce secure onboarding UX; only then enable the first protected inbox APIs.

## Pull-request checklist

- Confirm latest main SHA, open PRs, current file tree before starting
- Explain why the change is not already implemented
- Avoid dependency additions that do not serve the chosen vertical slice
- Tests and CI green, or explicitly state blockers
- Inspect unauthorized/cross-tenant behavior
- Update this file with evidence only

## Verification limitations

This foundation was authored in an environment without .NET SDK and without npm network access, so neither dotnet test nor npm build was executed locally. GitHub Actions is the intended first executable check; review its result before merging.
