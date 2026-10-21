# BatuSales implementation ledger

_Last reviewed: 2026-10-21. Baseline: `main@08fef80b8e14d1627a0fbad5348f9da872548fa7` (README only)._

## Evidence-based milestones

| Milestone | Priority | State | Evidence |
|---|---|---|---|
| Monorepo baseline and developer documentation | P0 foundation | Proposed in this PR | README, project layout, Docker Compose |
| API liveness / database readiness endpoints | P0 foundation | Proposed in this PR | `/health/live`, `/health/ready` |
| Basic web app shell | P0 foundation | Proposed in this PR | `apps/web`; roadmap-only page |
| API smoke tests and CI | P0 foundation | Proposed in this PR | xUnit tests, GitHub Actions |
| Auth, secure onboarding and tenancy | P0 | Not started | Must precede customer data APIs |
| CRM and customer records | P0 | Not started | Require server-enforced tenant scope |
| Web chat, inbox and human handoff | P0 | Not started | Require auth and event model |
| Knowledge base, agent and tool constraints | P0 | Not started | Evaluation, guardrails, approval gates |
| Booking and conflict-free scheduling | P0 | Not started | PostgreSQL transactions/constraints |
| Billing, quotas and audit log | P0 | Not started | Provider and region decision required |

**Do not treat this list as completion proof:** only merge + successful CI + manual verification can change a proposed row to implemented.

## Next implementation target

**P0-01: Authentication, onboarding, and tenant-bound persistence**

Acceptance criteria:
1. Secure register/login/refresh/logout and password recovery flows with abuse protection.
2. Business owner can create exactly one business via initial onboarding transaction.
3. Server resolves tenant membership from authenticated identity (never from a client-supplied tenant header).
4. EF Core PostgreSQL migrations create `tenants`, `users`, `memberships` and initial audit records.
5. Cross-tenant read/write tests, including direct ID-guessing attempts, prove isolation.
6. Secrets never appear in git or application logs; authorization must fail closed.
7. README and this ledger updated only once each change is validated.

## Technology radar (2026-10-21)

- **Adopt:** .NET 10 LTS, ASP.NET Core native health checks and rate limiting; Next.js 16 App Router; PostgreSQL 18.
- **Evaluate with a real need:** pgvector for RAG, OpenTelemetry for tracing, managed Postgres and object storage, EF Core 10 and Npgsql 10.
- **Defer:** preview .NET 11 / PostgreSQL 19, distributed microservices, RabbitMQ, Redis, workflow builder, self-hosted LLMs.
- **Open risks:** first customer segment; jurisdiction-specific payment options and KVKK/GDPR assessment; build reproducibility (`package-lock.json`); readiness and security testing.

## Repeat-safe development protocol

1. Query repository head and compare the prior recorded SHA with new commits.
2. Inspect branches, open PRs, issue scope, and tests before writing files.
3. Verify new feature is not already on main or in a pending PR.
4. Check official releases and advisories; record only upgrades with demonstrated value.
5. Implement on a new branch; prefer one topic / one reviewable commit.
6. Run automated tests and review security implications.
7. Update this ledger with branch/commit/PR and evidence; never mark proposed features done prematurely.
