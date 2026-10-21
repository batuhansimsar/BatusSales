# P0 auth email gate — candidate #8 add-on

Reviewed: 2026-10-21
Base: PR #8, commit `ccae2889c6f4751616623caabc6f48495a293cac`.
This is a **dependent draft change** and not a production capability.

## Why

ASP.NET Core Identity API routes include account confirmation and password
recovery; the baseline branch had no explicit `IEmailSender<ApplicationUser>`
registration. This add-on registers an intentional no-op implementation **only**
in the Development environment, with redacted diagnostic messages.
It also rejects non-development startup until a real transactional email sender
and its verified flow are implemented. The development flow deliberately permits
login without email confirmation, because emails are not delivered.

## Verified by source/test

- Identity email confirmation is required outside local Development.
- Confirmation/reset URLs, codes and recipient addresses are never logged.
- A named production/staging environment cannot silently use the no-op sender.
- Unit tests exercise both option policies and sensitive-log suppression.

## Not solved

- Real mail delivery, authenticated confirmation and recovery E2E tests.
- Device/session revocation and browser BFF/CSRF design.
- Reviewed PostgreSQL migrations, organization/tenant negative HTTP tests,
  backup/restore and production deploy configuration.
- Nothing should merge before a canonical backend is selected from competing
  PRs #4, #5, #8, #11 and #12. Do not merge this child PR independently of #8.

## External guidance

https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0
https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iemailsender-1?view=aspnetcore-10.0
