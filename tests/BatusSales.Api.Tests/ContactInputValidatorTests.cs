using BatusSales.Api.Validation;

namespace BatusSales.Api.Tests;

public sealed class ContactInputValidatorTests
{
    [Fact]
    public void RejectsMissingName()
    {
        var errors = ContactInputValidator.Validate(new ContactInput(" ", "valid@example.com", null));
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public void RejectsInvalidEmail()
    {
        var errors = ContactInputValidator.Validate(new ContactInput("Customer", "not-an-email", null));
        Assert.Contains("email", errors.Keys);
    }

    [Fact]
    public void RejectsLongPhone()
    {
        var errors = ContactInputValidator.Validate(new ContactInput("Customer", null, new string('1', 51)));
        Assert.Contains("phone", errors.Keys);
    }

    [Fact]
    public void AcceptsValidContact()
    {
        var errors = ContactInputValidator.Validate(new ContactInput("Customer", "valid@example.com", "+90 555 111 22 33"));
        Assert.Empty(errors);
    }
}
