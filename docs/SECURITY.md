# Phase 0 security boundary (not production certified)

- Authentication uses ASP.NET Core Identity API bearer tokens.
- Every CRM endpoint requires authentication, membership for the URL's organization, and a database query filtered by that organization ID.
- Unknown organizations and out-of-tenant contacts return 404 rather than exposing foreign records.
- Organization creation and Owner membership are committed in one database transaction.
- Contact create/update writes a minimal audit record without including customer message/PII values.
- Identity endpoints are rate-limited per remote IP in this development configuration. Behind a reverse proxy, configure forwarded headers and a trusted proxy list before launch.
- No arbitrary AI/model tool execution is included. Future agent actions must enforce tenant authorization independently of model-generated arguments.
- PostgreSQL RLS is **not** configured in this increment; do not treat URL filters as the complete final isolation model.
- Verified email, MFA, token revocation strategy, integration tests, database migrations, retention/deletion requests, secrets management, secure cookies, traffic proxies, backup/restore and audit review must be completed before onboarding real customers.
