# Technology radar — reviewed 2026-10-21

| Technology | Decision | Why / next trigger |
| --- | --- | --- |
| .NET 10 LTS + ASP.NET Core | **Adopt** | Supported through Nov 2028; .NET 8/9 support ends Nov 2026. |
| EF Core 10 + Npgsql 10.0.3 | **Adopt** | Stable .NET 10-compatible PostgreSQL provider. Pin and monitor patches. |
| PostgreSQL | **Adopt** | Source of truth, transactional CRM and booking writes. |
| Next.js + TypeScript | **Planned M1/M2** | UI when authenticated API contracts exist; pin stable version at implementation. |
| pgvector / RAG | **Evaluate M3/M7** | Start with controlled FAQ/citation rules and a baseline before embedding data. |
| Microsoft Agent Framework | **Evaluate, not installed** | Supports agents, workflows, tool approval and multi-provider integration. Initial MVP may need only a narrowly scoped gateway. |
| OpenTelemetry | **Planned** | Trace HTTP, DB and LLM cost/success metrics with PII-redacted attributes. |
| Redis/RabbitMQ/MassTransit | **Deferred** | Add only when job delivery/retries and throughput warrant extra moving parts. |
| MCP tool access | **Deferred** | Adds useful integrations but also permissions and supply-chain risk; isolate and audit. |
| .NET 11 RC | **Do not adopt** | Prefer supported stable LTS for early paying customers. |

Official starting points:
- https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/10.0.3
- https://learn.microsoft.com/en-us/agent-framework/overview
- https://genai.owasp.org/initiatives/top-10-for-llm-and-genai/

Every development session: compare this radar with current official docs; record a source and concrete benefit before adding a dependency.
