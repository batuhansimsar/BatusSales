# BatuSales development status

_Last reviewed: 2026-10-21. Baseline main commit: `08fef80b8e14d1627a0fbad5348f9da872548fa7`._

## Verified starting state

The repository contained one `README.md`, one initial commit, no open issues
and no open PRs. No application source code existed.

## First implementation slice (this PR)

| Area | Status | Scope |
|---|---|---|
| Solution foundation | Implemented in PR | ASP.NET Core 10 API, PostgreSQL dev compose, CI |
| Tenant identity | Partial | External signed JWT validation, organization + owner membership |
| Tenant isolation | Partial | Membership check, contact read filter, write guard + unit tests |
| CRM | Partial | Create/list/read contacts; no edit, tags, pipeline or reminders |
| Authentication UI | Not started | External OIDC provider configuration required |
| AI agents / knowledge base | Not started | No LLM or RAG integration |
| Web chat / inbox | Not started | No widgets, agent handoff or channels |
| Bookings | Not started | No resource calendar or collision protection |
| Workflows | Not started | No trigger/action engine |
| Billing | Not started | No subscription or metering |
| Dashboard | Not started | No aggregates, performance reporting |
| Admin / marketing / integrations | Not started | Deferred until core P0 flow is useful |

**This is not a completed MVP.** Do not describe first-slice infrastructure as
shipped customer-facing capability.

## Next tasks in priority order

1. Provision an OIDC provider, add authenticated integration tests with PostgreSQL
   and verify anonymous, cross-tenant, revoked membership and role scenarios.
2. Add versioned EF Core migrations, tenant-bound DB access and PostgreSQL RLS
   evaluation, audit logs, rate limits and centralized exception handling.
3. Build Next.js 16.4 onboarding and a usable contacts/inbox shell with responsive UX.
4. Build web chat widget, durable inbound messages and human handoff.
5. Implement one FAQ-backed AI agent with refusals, citations, bounded tools and
   human escalation. Add evaluation scenarios and per-tenant AI budget caps.
6. Add appointments with database-enforced resource/time collision safeguards.
7. Add basic workflow triggers; only later connect real billing and external channels.
8. Validate a single industry (e.g. beauty salons) with 10–20 discovery interviews
   and 2–3 paid pilots before expanding scope.

## Verification still needed

- This branch includes GitHub Actions for .NET 10 build and unit tests.
- Local environment used to prepare the changes did not provide the .NET SDK,
  so a successful local compilation is **not** claimed.
- Run Postgres-backed integration tests and check Actions results before merging.

## Completion definition per future change

Feature exists in code + explicit tests + migration where relevant + documentation
+ passing CI. Review existing files, branch tips, merged/unmerged PRs and history
before creating a new commit; never replay an already-applied patch.
