using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class IdentityEmailGateTests
{
    [Fact]
    public void Non_development_environments_require_confirmed_email()
    {
        var options = new IdentityOptions();
        IdentityEmailPolicy.Configure(options, isDevelopment: false);
        Assert.True(options.SignIn.RequireConfirmedEmail);
    }

    [Fact]
    public void Local_development_allows_unconfirmed_email_for_disposable_test_accounts()
    {
        var options = new IdentityOptions();
        IdentityEmailPolicy.Configure(options, isDevelopment: true);
        Assert.False(options.SignIn.RequireConfirmedEmail);
    }

    [Fact]
    public async Task Development_sender_does_not_log_email_addresses_or_recovery_secrets()
    {
        var logger = new RecordingLogger();
        var sender = new DevelopmentEmailSender(logger);
        var user = new ApplicationUser();
        const string recipient = "private@example.test";
        const string confirmationLink = "https://example.test/confirm?code=SECRET_CONFIRM";
        const string resetLink = "https://example.test/reset?code=SECRET_RESET";
        const string resetCode = "SECRET_123456";

        await sender.SendConfirmationLinkAsync(user, recipient, confirmationLink);
        await sender.SendPasswordResetLinkAsync(user, recipient, resetLink);
        await sender.SendPasswordResetCodeAsync(user, recipient, resetCode);

        Assert.Equal(3, logger.Messages.Count);
        var output = string.Join("\n", logger.Messages);
        Assert.DoesNotContain(recipient, output);
        Assert.DoesNotContain(confirmationLink, output);
        Assert.DoesNotContain(resetLink, output);
        Assert.DoesNotContain(resetCode, output);
    }

    private sealed class RecordingLogger : ILogger<DevelopmentEmailSender>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
            EmptyScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class EmptyScope : IDisposable
        {
            public static readonly EmptyScope Instance = new();
            public void Dispose() { }
        }
    }
}
