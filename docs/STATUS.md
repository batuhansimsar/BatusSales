# BatuSales — development status

Review date: 2026-10-21
Repository: batuhansimsar/BatusSales
Baseline main commit before this change: 08fef80b8e14d1627a0fbad5348f9da872548fa7
Current workstream: feat/foundation-auth-tenancy-crm-v1

## Shipped in this workstream (pending review/CI)
- .NET 10 API bootstrapping with PostgreSQL and rate limits.
- ASP.NET Core Identity registration/login/refresh endpoints.
- Authenticated organization onboarding and membership listing.
- Owner membership creation with new organization.
- Tenant membership endpoint guard (fail closed) for CRM.
- Contact create/list/get with explicit tenant ID on writes and EF query filters.
- Tenant-isolation test cases and CI workflow.
- Development-only database initialization; Docker PostgreSQL.

## Not yet shipped / do not claim complete
- Database migrations and tested production deployment.
- Confirmed-email registration, recovery email sender, full Owner/Admin/Agent RBAC.
- Contact update/delete, deals, pipeline, lead scores.
- Dashboard UI and Next.js frontend.
- Web widget, inbox, AI agents, knowledge base, RAG.
- Booking, workflow, marketing, billing, audit logs, telemetry, backup/restore.
- Tenant-level PostgreSQL RLS and end-to-end security tests.
- Subscription usage quotas and super-admin tools.

## Next action
1. Check PR CI; correct any build or test failures.
2. Generate and review an initial EF migration; remove reliance on EnsureCreated.
3. Implement authenticated team roles/permissions and tenant E2E tests.
4. Deliver first web chat-to-inbox-to-CRM vertical slice, then booking.
5. Run real customer demos before expanding to P1.

## Quality / validation notes
The code here was authored without a local .NET SDK in the execution environment.
Compiler, integration and runtime success are NOT yet verified. GitHub CI must be checked.
