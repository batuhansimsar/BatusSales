# P0 CRM Contact Deletion Authorization

Reviewed: 2026-10-21. Parent: `feat/mvp-foundation-tenant-crm` at `18a0ccfcf983`.

## Narrow improvement
- Organization membership is required for any contact endpoint.
- Soft-delete is more sensitive than routine contact access and is restricted to Owner/Admin.
- Agent members get 403 for delete attempts; non-members get 404 to avoid tenant enumeration.
- Role is read from server-side membership storage for the authenticated principal and exact organization.
- Adds unit checks for owner/admin/agent permissions, cross-organization membership, unknown identity and empty organization ID.

## Not done / release blockers
- No frontend or migration change; no production rollout.
- CI must pass; authenticated HTTP integration tests against PostgreSQL are still required.
- Audit privileged deletes, ensure stale/revoked memberships are rejected, and check role permission requirements with pilot customers.
- Endpoint membership checks do not replace database tenant isolation and composite foreign keys.
- Keep production registration email verification, CSRF and data retention checks in the release gate.

## Engineering guidance
Avoid adding a generic authorization framework or AI-specific library for a single permission boundary. Expand into reusable policy-based authorization when more actions need distinct roles.

Security background: https://owasp.org/Top10/ and https://genai.owasp.org/llm-top-10/
