using System.ComponentModel.DataAnnotations;

namespace BatusSales.Api.Validation;

public sealed record ContactInput(string? Name, string? Email, string? Phone);

public static class ContactInputValidator
{
    public static Dictionary<string, string[]> Validate(ContactInput input)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 160)
            errors["name"] = ["Name is required and must be 160 characters or fewer."];

        if (!string.IsNullOrWhiteSpace(input.Email) &&
            (input.Email.Trim().Length > 320 || !new EmailAddressAttribute().IsValid(input.Email.Trim())))
            errors["email"] = ["Provide a valid email address of at most 320 characters."];

        if (input.Phone?.Trim().Length > 50)
            errors["phone"] = ["Phone must be 50 characters or fewer."];

        return errors;
    }
}
