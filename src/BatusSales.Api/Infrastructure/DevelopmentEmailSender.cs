using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Infrastructure;

// Local development only: never substitute this for transactional email in production.
public sealed class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    : IEmailSender<AppUser>
{
    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        logger.LogWarning("Development sender discarded confirmation link.");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        logger.LogWarning("Development sender discarded reset link.");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        logger.LogWarning("Development sender discarded reset code.");
        return Task.CompletedTask;
    }
}
