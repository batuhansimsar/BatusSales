# Security baseline and known gaps

## Foundation guarantees (when implemented and validated)
- No unauthenticated route can currently read/write business data; only public liveness and readiness.
- Tenant-scoped EF models use fail-closed query filters and SaveChanges guards. They are **defense in depth**, not authentication.
- A tenant context can be initialized once per request, but its initializer **must only** be called after verifying logged-in user membership. It must never trust X-Tenant-ID, query strings, prompts or tool output.
- No API keys or payment credentials in source control; configuration via runtime secret management.
- Database connection detail is omitted from external health response.

## NOT yet implemented (block production launch)
- AuthN, membership-level AuthZ, owner/admin/agent RBAC; account session protection, CSRF assessment.
- Database migration and composite tenant-scoped foreign keys. Query filters alone do not prevent unsafe cross-tenant FK relationships or arbitrary raw SQL.
- Row-level security evaluation, authenticated integration tests against real PostgreSQL.
- Atomic, DB-enforced prevention of overlapping bookings; BookingWindow only checks a logical interval.
- Inbound rate limits, API schemas, audit, consent/deletion, PII retention, tenant-aware encrypted backups, restore drill.
- Prompt-injection adversarial tests, least-privilege tools, approval gates, provider cost caps.
- TLS termination, secure headers, production deployment/cookie/CORS policies.

## Pre-launch security checks
1. Exercise every authenticated endpoint with owner/admin/agent across two organizations.
2. Confirm `IgnoreQueryFilters`, raw SQL and admin/support routes have explicit authorization and audit.
3. Test replay, webhook signature failure, tool escalation, invoice forgery, prompt injection, and cost exhaustion.
4. Validate database constraints, backups and incident recovery in staging.

References: https://genai.owasp.org/initiatives/top-10-for-llm-and-genai/ and https://owasp.org/projects/mcp-top-10/
