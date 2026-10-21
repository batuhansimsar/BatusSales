# BatuSales repository operating rules

Before making ANY subsequent change:

1. Inspect `git status`, current branch, `git log --oneline -15`, open PRs, and `docs/STATUS.md`. Confirm the task is not already present in main or an open branch.
2. Read the actual relevant source files, not merely the roadmap. Explicitly distinguish implemented/tested vs. planned.
3. Verify live official documentation for relevant dependency versions, security advisories, API behavior, and compatibility before suggesting a new technology. Record the trade-off in `docs/TECH_DECISIONS.md`.
4. Choose the smallest P0 vertical slice that unblocks first paid pilot. Keep modular monolith. Do not add a new service, queue, cache, agent framework, or library without evidence that the slice needs it.
5. Create a feature branch (never push a risky change directly to main). Do not recreate the same commit or submit duplicate code. Compare diff to base.
6. Require membership checks and explicit organization scope on every tenant resource. Add tests for boundaries and regressions. Do not log customer PII or secrets.
7. Run build/tests, record commands and results (or state truthfully when execution was unavailable), and create a reviewable PR.
8. Update `docs/STATUS.md` with completed, pending, and blocked work, using links to commits/PRs; do not mark unverified work as done.

Security release gate: migrations, verified email, production TLS/proxy settings, tenant boundary tests, RLS where appropriate, back-up restore test, logging/retention, and AI tool authorization MUST be reviewed before first real-customer onboarding.
