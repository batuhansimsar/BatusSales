# Architecture and trust boundaries

## ADR-001: Begin with a modular monolith

Use a single ASP.NET Core backend with explicit feature/module boundaries. Avoid network service boundaries until traffic and operational evidence justify them. Frontend is Next.js App Router with TypeScript and Tailwind.

Initial planned modules: Identity, Tenancy, Organizations, Inbox, Agents, KnowledgeBase, CRM, Booking, Automation, Billing, Analytics, Admin.

## ADR-002: Tenant ID is not authorization

A valid TenantId only represents an identifier; it NEVER grants access. Future authenticated endpoints must resolve tenant context from the signed authenticated identity AND current membership stored on the server. Never accept a raw caller-supplied tenant header as proof. Filter reads by tenant; validate the tenant owner of every referenced resource on writes. Exercise cross-tenant negative tests before exposing protected endpoints.

Do not treat EF global query filters alone as a complete security boundary; ensure admin paths, raw SQL, background jobs, cache keys, storage paths and exports explicitly enforce tenant isolation. Consider PostgreSQL row-level security for an additional defense after the initial model is established.

## ADR-003: Do not expose unsafe AI tools

AI-generated text does not authorize account, payment, deletion, or outbound campaign actions. Keep tool invocations behind business APIs with explicit scopes, review gates, budgets, idempotency and audit trails. Never construct authorization decisions from prompt text.

## ADR-004: Do not ship fake business integrations

The homepage is a status/landing scaffold, not a working dashboard. The API health proxy exposes only availability and never customer data. No CRM, AI, booking or billing endpoint is public yet.

## Runtime separation

- Browser -> Next.js routes -> ASP.NET API over private service URL.
- API secrets are server-side only and must never use NEXT_PUBLIC_ prefixes.
- TLS terminates at trusted infrastructure for production; configure trusted forwarded headers before enabling HTTPS assumptions in containers.
- Frontend and backend do not enable unrestricted cross-origin access.
- Public health routes are deliberately minimal and rate-limited; no diagnostic secrets in output.

## Future persistence

PostgreSQL + EF Core 10; consider separate schema by domain. Adopt normalized migrations, tenant-scoped indexes and concurrency control. Redis and MassTransit/RabbitMQ are deferred until actual asynchronous workflows require them, to avoid unnecessary operational overhead.
