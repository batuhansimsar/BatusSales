# BatuSales delivery ledger

Audit baseline: 2026-10-21 | main @ 08fef80 (only README)
Initial foundation branch: feat/p0-api-tenancy-crm-foundation

| Feature | Priority | Current state | Acceptance gate |
|---|---|---|---|
| API foundation + health probes | P0 | Proposed in foundation PR | Build & boot with documented configuration |
| Signed external JWT validation | P0 | Proposed in foundation PR | Valid/invalid token integration tests |
| Tenant claim + database membership enforcement | P0 | Proposed in foundation PR | Cross-tenant DB integration tests, audited RLS strategy |
| Tenant-scoped CRM contacts | P0 | Proposed in foundation PR | Authenticated create/list/read E2E; pagination, audit |
| Organization onboarding | P0 | Not started | Tenant provisioning and invitation flow |
| Web chat widget + shared inbox | P0 | Not started | Conversation-to-operator demo |
| AI agent + basic knowledge | P0 | Not started | Grounded answers, refusal and human handoff |
| Booking calendar | P0 | Not started | Concurrent booking/availability test |
| Simple workflow triggers | P0 | Not started | Idempotent reliable action demo |
| Usage and billing | P0 | Not started | Quotas, payment webhook idempotency |
| Web console (Next.js) | P0 | Not started | Login and tenant dashboard flow |
| WhatsApp / Instagram / RAG | P1 | Not started | Official API & policy review |
| Voice / SSO / Enterprise | P2 | Not started | Proven demand + security review |

## Recommended next vertical slices (not yet completed)

1. Replace manual membership setup with a reviewed, authenticated organization
   onboarding & invitation flow; add DB-level tenant integration tests.
2. Build Next.js 16 frontend shell with explicit authenticated organization
   selection and CRM contact page; do not expose privileged tokens to the client.
3. Build one working web-chat-to-inbox flow before integrating an LLM.
4. Add tool-restricted AI responses only after tenant and audit foundations pass.
5. Then implement bookings, quotas and billing, targeting a single service niche.

## Repeat-work prevention checklist

- [ ] Inspect default branch HEAD SHA and file tree.
- [ ] Inspect open PRs; do not implement unmerged work again.
- [ ] Compare status acceptance criteria with actual code/tests, not claims.
- [ ] Verify latest supported versions/security advisories.
- [ ] Use a new scoped branch, do not push directly to main.
- [ ] Update this ledger after tests and review; never mark unverified work done.
