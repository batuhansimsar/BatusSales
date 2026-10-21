using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Infrastructure;

/// <summary>
/// Local development only. No messages, confirmation links or reset secrets are sent or logged.
/// Production startup refuses this implementation until a real sender is configured.
/// </summary>
public sealed class DevelopmentEmailSender : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(
        ApplicationUser user, string email, string confirmationLink) => Task.CompletedTask;

    public Task SendPasswordResetLinkAsync(
        ApplicationUser user, string email, string resetLink) => Task.CompletedTask;

    public Task SendPasswordResetCodeAsync(
        ApplicationUser user, string email, string resetCode) => Task.CompletedTask;
}
