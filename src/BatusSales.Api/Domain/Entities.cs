namespace BatusSales.Api.Domain;

public interface ITenantOwned
{
    Guid OrganizationId { get; set; }
}

public sealed class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public abstract class TenantEntity : ITenantOwned
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class Contact : TenantEntity
{
    public required string DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public enum ConversationState { Open, Pending, Resolved }

public sealed class Conversation : TenantEntity
{
    public Guid ContactId { get; set; }
    public ConversationState State { get; set; } = ConversationState.Open;
    public string Channel { get; set; } = "web";
}

public enum MessageAuthor { Visitor, HumanAgent, AiAgent }

public sealed class ConversationMessage : TenantEntity
{
    public Guid ConversationId { get; set; }
    public MessageAuthor Author { get; set; }
    public required string Body { get; set; }
}

public enum BookingState { Requested, Confirmed, Cancelled }

public sealed class Booking : TenantEntity
{
    public Guid ContactId { get; set; }
    public Guid StaffId { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public BookingState State { get; set; } = BookingState.Requested;
}
