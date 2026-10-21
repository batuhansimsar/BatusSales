# MVP-to-revenue roadmap

## M0 — Foundation (this PR)
.NET 10 API, PostgreSQL wiring, tenant-bound EF query filters and write guards, booking interval domain rule, health checks, tests, CI, Docker, status journal.

**Acceptance:** CI green; no public tenant-data endpoints; no business feature claimed complete.

## M1 — Identity + Organizations (P0)
ASP.NET Core Identity or equivalent hardened identity; session lifecycle; business onboarding; invite/member roles; membership verified **server side** before scoped tenant binding; rate limiting; audit; initial migration; authenticated API integration tests.

## M2 — Web chat and inbox (P0)
Embeddable widget with restricted allowed origins, public widget key separation, per-widget rate quotas, input caps, visitor session ownership, canonical message persistence, message read/assignment/resolve, SignalR, handoff.

## M3 — AI assisted replies (P0)
Business-scoped FAQ/catalogue, model-independent client, grounded answers and abstention, human handoff, allowlisted tools, token/cost budgets, deterministic audit events, adversarial test set.

## M4 — CRM and booking (P0)
Contacts + lead pipeline, authorized endpoints, staff/services, UTC booking with local zone display, database-enforced overlap prevention, transactional booking creation, cancellation rules, E2E tests.

## M5 — Revenue and core automation (P0)
Measured usage entitlements, plan enforcement, verified billing webhooks, trial management, smallest reliable event->condition->action workflow, retry/idempotency and dead-letter strategy.

## M6 — Customer-facing MVP
Dashboard, complete onboarding, responsive frontend, integration tests in real PostgreSQL, backup/restore drill, rate limits, legal/privacy review, 2–3 paid pilots.

## M7 — Customer-driven growth (P1)
Prioritize official WhatsApp/Instagram and calendar connectors, RAG/pgvector, templates, segmentation, agent evaluation from real pilot feedback. No speculative enterprise complexity.

**Definition of Done:** code merged, CI green, documented, tenant leakage checks, measured cost impacts, and a reproducible business flow. P0 scope is defined by the product spec, not by this single PR.
