# BatusSales — agent collaboration rules

These instructions apply to every contribution to this repository. Canonical live repository: https://github.com/batuhansimsar/BatusSales. Product roadmap: **P0 sellable MVP**, then P1 growth and P2 enterprise. Read `docs/PROJECT_STATE.md` and coordination issue [#14](https://github.com/batuhansimsar/BatusSales/issues/14) before making changes.

## Mandatory per-session preflight (fresh data, never cached)

1. Read the **current default branch HEAD SHA** and tree, changed files, latest commits, `docs/PROJECT_STATE.md`, all open **and relevant closed/merged PRs**, issues, and CI results. They can change *during* a session because multiple agents operate concurrently. Re-check before push, merge, and final report.
2. Classify any requested capability as **MERGED / IN OPEN PR / MISSING / BLOCKED** based on actual source and tests, not PR titles or summaries.
3. Search both the canonical main tree and every relevant PR/branch for the same endpoint, service, migration, test case, docs file, or security invariant. **Do not commit the same change twice.**
4. Check current official .NET, EF Core, PostgreSQL, Next.js, NuGet advisories, GitHub Actions and OWASP guidance that is relevant to the chosen increment. Do not chase novelty without a measurable product need.
5. Choose the smallest *distinct* P0 increment with direct pilot value or security necessity. Prefer adding it to an existing appropriate **open PR/workstream**, not a new duplicate foundation or identical regression test. If several PRs already target the same behavior, review/consolidate them rather than adding a third.
6. Before editing, re-check the target branch head SHA; do not force push, clobber other agents' changes, duplicate commits or silently rewrite branch history. Use additive, reviewable commits with tests.
7. Verify build, unit, integration and security CI on the *current* commit; check the merge result and subsequent main HEAD if merged. A past green workflow is **not** proof that a newer commit passed. If tools or CI are unavailable, report the limitation and leave the change explicitly unverified.
8. Update the **single** status ledger (not competing status documents) when state materially changes: merged SHA, proof/test links, open PR ownership, release blockers, next distinct work. Do not append stale historical claims as current facts.

## Canonical architecture / no second backend

- The canonical .NET 10 / PostgreSQL development Identity–Organizations–CRM foundation was merged via [#12](https://github.com/batuhansimsar/BatusSales/pull/12); the security logic from #15/#16 is in main. [#21](https://github.com/batuhansimsar/BatusSales/pull/21) added live membership-revocation HTTP regression. **Never scaffold a competing `backend/`, `src/BatuSales.Api`, or `src/BatusSales.Api` solution to re-implement these.**
- Main backend path: `src/BatusSales.Api`; tests: `tests/BatusSales.Api.Tests`, `tests/BatusSales.Api.IntegrationTests`.
- Distinct open drafts may include #1 booking domain and #2/#7 UI references. Compare and port unique ideas to canonical paths, not entire parallel scaffolds.
- Multiple active security PRs may cover the *same* role downgrade, audit isolation, or auth edge case. **One proven invariant = one canonical test**, unless an additional test provides a genuinely different layer (unit/SQLite/PostgreSQL HTTP) or threat scenario.

## P0 security and release gates

- All tenant/contact authorization must be **server-verified** from current membership/role; never trust client-provided tenant IDs, headers, or claims alone.
- Keep EF default-deny tenant filters and guarded tracked writes; remember `IgnoreQueryFilters`, raw SQL and bulk `ExecuteUpdate/Delete` can bypass EF safeguards. Evaluate PostgreSQL RLS and cross-tenant negative integration tests before real customer data.
- Confirmed-email production onboarding, robust auth/session/CSRF design, reviewed EF Core **migrations** (do not ship `EnsureCreated`), secret management, rate limits, consent/retention/deletion, backups/restore, budgeted AI tools, logging/monitoring and CI are still release work.
- Booking must enforce cross-tenant isolation, calendar/time-zone correctness, and database-level overlap prevention under concurrent requests. AI must use constrained tools, grounded business knowledge and explicit human authorization for high-impact actions.
- Neither a prototype, a passing isolated unit test nor a merged foundation makes the project ready for production, real customer data or paid pilots.

## Output of every development session

Record exact before/after main SHA, the existing work reused, the new distinct change (if any), PR/commit links, CI names/conclusions tied to exact SHA, risks and the highest-value unfinished P0 next step. Be concise and explicit about incomplete work.
