# BatusSales contribution instructions

These instructions apply to every coding agent and contributor working in this repository.

## Before writing any code
1. Read the CURRENT default-branch SHA, latest commits, open/closed PRs, active branches, GitHub Actions results, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`, `docs/SECURITY.md`, and `docs/TECH_RADAR.md`.
2. Search for the requested feature by module, type, endpoint and tests. Compare open PR diffs; **an unmerged PR is work in progress, not a missing feature and not completed production functionality**.
3. If another active PR already addresses the same module, do not create a competing implementation. Review or continue the existing branch instead, after checking its head SHA and permissions.
4. Check official release/security documentation for technologies impacted by the task, record date/sources in the tech radar. Do not add dependencies for novelty alone.
5. Start from the latest compatible base. Never reuse a branch name to overwrite unrelated changes or repeat identical commits.

## Implementation requirements
- Build one vertical slice at a time. Priority: P0 sellable MVP, then evidenced P1/P2 demand.
- Maintain modular monolith and tenant data boundaries. Never accept tenant IDs from arbitrary HTTP headers or AI text without independently verified authorization.
- Enforce authorization in endpoints and data access. Tenant filters are defense-in-depth, not a substitute for identity/membership checks.
- High-risk AI tool actions need least privilege, explicit human approval and auditable state transitions.
- No credentials in git; validate inputs, cap usage, use idempotency for message/payment/booking actions.
- Update tests for changes, including negative and cross-tenant tests; maintain reproducible CI.
- Do not call a feature complete without merged code, green CI and verified behavior.

## After implementation
1. Run relevant build, tests and migration checks (or clearly report why they could not run).
2. Update `docs/PROJECT_STATUS.md` with verified states and precise remaining blockers.
3. Open a reviewable PR with scope, test result, risk analysis and next step. Prefer draft while CI/security conditions are incomplete.
4. Recheck base HEAD and competing PRs before merge. Never merge multiple competing project scaffolds directly.

**2026-10-21 coordination warning:** PRs #1 through #8 were started in parallel from a README-only baseline; compare and consolidate them before merging. See PR #6's reconciliation comment. This note must be refreshed as PRs are resolved.
