using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Infrastructure;

/// <summary>
/// Explicit development-only no-op. Never register in staging or production.
/// Does not log recipients, reset codes, or confirmation URLs.
/// </summary>
public sealed class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        Discard("confirmation");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        Discard("password-reset-link");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        Discard("password-reset-code");

    private Task Discard(string messageType)
    {
        logger.LogWarning("Development-only Identity {MessageType} email suppressed; nothing sent.", messageType);
        return Task.CompletedTask;
    }
}
