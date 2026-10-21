using System.ComponentModel.DataAnnotations;
using BatusSales.Api.Domain;

namespace BatusSales.Api.Features.Contacts;

public sealed record ContactRequest(string? Name, string? Email, string? Phone);
public sealed record ContactResponse(
    Guid Id, Guid OrganizationId, string Name, string? Email, string? Phone,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static ContactResponse From(Contact item) =>
        new(item.Id, item.OrganizationId, item.Name, item.Email, item.Phone,
            item.CreatedAtUtc, item.UpdatedAtUtc);
}

public static class ContactValidation
{
    public static Dictionary<string, string[]> Validate(ContactRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
            errors["name"] = ["Name must be between 1 and 120 characters."];

        var email = request.Email?.Trim();
        if (!string.IsNullOrEmpty(email)
            && (email.Length > 254 || !new EmailAddressAttribute().IsValid(email)))
            errors["email"] = ["Email must be a valid address up to 254 characters."];

        if (!string.IsNullOrEmpty(request.Phone) && request.Phone.Trim().Length > 40)
            errors["phone"] = ["Phone must be at most 40 characters."];

        return errors;
    }
}
