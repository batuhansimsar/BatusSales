# PostgreSQL tenant integration checks (P0)

Status: added to the **draft stacked integration PR**, not merged or production-ready.

This is a separate xUnit test project using ASP.NET Core WebApplicationFactory and a
real PostgreSQL 18 container in GitHub Actions. The original fast InMemory unit tests
remain unchanged.

## What it proves once CI passes

- Identity register/login yields a token accepted by protected HTTP endpoints.
- An authenticated user can create an organization and CRM contact.
- A separately authenticated user's cross-organization list, get, create,
  update, and delete requests are rejected.
- Anonymous contact reads are rejected.
- Removing a membership in PostgreSQL invalidates authorization on the next
  request, despite the caller still holding an existing access token.

## Run locally

Use a **disposable** database; the current development app calls EnsureCreated.
Never point this test project at production or shared customer data.

Example:

    docker run --rm -d --name batussales-integration-postgres \
      -e POSTGRES_USER=batussales_test \
      -e POSTGRES_PASSWORD=local_ci_only_password \
      -e POSTGRES_DB=batussales_test \
      -p 5433:5432 postgres:18-alpine

    export BATUSSALES_TEST_POSTGRES_CONNECTION='Host=localhost;Port=5433;Database=batussales_test;Username=batussales_test;Password=local_ci_only_password'
    dotnet test tests/BatusSales.Api.IntegrationTests/BatusSales.Api.IntegrationTests.csproj
    docker stop batussales-integration-postgres

This intentionally fails if BATUSSALES_TEST_POSTGRES_CONNECTION is missing;
it does not turn an unexecuted database test into a false success.

## Important limits

This verifies the HTTP authorization behavior of one selected candidate foundation,
not database RLS, migrations, concurrent writes or defense-in-depth against direct
unscoped DbContext use. Tenant checks are implemented at the endpoint layer in
this candidate. Shipping requires a chosen canonical architecture, robust EF guard
or RLS defense-in-depth, reviewed migrations, email/identity hardening, retention,
backups and repeatable restore drills.

## Technology review (2026-10-21)

- .NET 10.0.13 is the October 2026 LTS servicing update:
  https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- Microsoft.AspNetCore.Mvc.Testing and WebApplicationFactory are first-party
  ASP.NET Core testing support.
- Docker/Testcontainers guidance recommends testing with production-like
  database engines over EF InMemory alone:
  https://dotnet.testcontainers.org/examples/aspnet/
- Postgres 18 in CI is aligned with the disposable development environment.
  A dedicated GitHub Actions service container avoids another runtime dependency.

This change does not pick/merge a canonical foundation; see issues #13 and #14
for that integration gate.
