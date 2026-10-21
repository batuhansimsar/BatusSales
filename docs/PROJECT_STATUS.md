# BatusSales development status

Last assessed: **2026-10-21**. Initial baseline commit was `08fef80` (README only); this foundation work is proposed in `feat/foundation-p0-20261021`. **Work on a PR branch is NOT a completed/merged P0 feature.**

| Capability | Priority | Current state | Next evidence needed |
| --- | --- | --- | --- |
| API scaffolding / health | P0 infrastructure | Implemented on foundation branch; CI pending | Build + test + merge |
| Organization onboarding / identity | P0 | Not started | Verified signup, membership, authorization tests |
| Tenant isolation | P0 | Query/write guard coded; auth binding + DB integration pending | Real PostgreSQL isolation test, membership gate |
| CRM contacts + pipeline | P0 | Data model draft only | Authenticated CRUD + migrations + E2E |
| Inbox + website widget | P0 | Message model draft only | Signed widget config, limits, inbox UI, human transfer |
| AI agent + knowledge | P0 | Not started | Grounded answers, handoff, model cost caps, evaluations |
| Booking calendar | P0 | Interval rule and tests drafted | Atomic double-booking protection, endpoints/UI |
| Automation | P0 | Not started | Safe event triggers + idempotent actions |
| Billing, trial, quotas | P0 | Not started | Provider sandbox, webhook verification, entitlements |
| Dashboard | P0 | Not started | Real tenant-scoped metrics and verified definitions |
| Admin + observability | P0 | Health endpoints only | Audit events, monitoring, support access policy |

## Rules for next session
1. Read default branch HEAD, PR list, active branches and GitHub Actions first.
2. If foundation PR is open, review its diff/CI before changing the same files.
3. Classify features as Not started / In progress / Tested / Merged / Verified in staging. Never mark an unmerged PR as complete.
4. Read **docs/TECH_RADAR.md**, verify changing dependencies with official sources, and record decisions with dates.
5. Implement the smallest next vertical slice and update this file in the same PR.

## Highest priority after this foundation
**Verified login + organization creation + membership-scoped authorization + first EF Core migration**. Only then expose CRM or booking endpoints.
