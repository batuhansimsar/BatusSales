using BatusSales.Api.Features.Contacts;

namespace BatusSales.Api.Tests;

public sealed class ContactValidationTests
{
    [Fact]
    public void RejectsMissingName()
    {
        var errors = ContactValidation.Validate(new ContactRequest(" ", null, null));
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public void RejectsInvalidEmail()
    {
        var errors = ContactValidation.Validate(new ContactRequest("Customer", "invalid-email", null));
        Assert.Contains("email", errors.Keys);
    }

    [Fact]
    public void AcceptsValidContact()
    {
        var errors = ContactValidation.Validate(
            new ContactRequest("Customer", "customer@example.com", "+905551234567"));
        Assert.Empty(errors);
    }

    [Fact]
    public void RejectsOversizedPhone()
    {
        var errors = ContactValidation.Validate(new ContactRequest("Customer", null, new string('1', 41)));
        Assert.Contains("phone", errors.Keys);
    }
}
