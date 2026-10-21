namespace BatusSales.Booking.Domain;

/// <summary>
/// An immutable appointment, uniquely identified within a tenant.
/// Times are normalized to UTC. The original service timezone belongs in the persistence/application layer.
/// </summary>
public sealed class Appointment
{
    private Appointment(
        Guid id,
        Guid tenantId,
        Guid staffId,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        AppointmentStatus status)
    {
        Id = id;
        TenantId = tenantId;
        StaffId = staffId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Status = status;
    }

    public Guid Id { get; }
    public Guid TenantId { get; }
    public Guid StaffId { get; }
    public DateTimeOffset StartUtc { get; }
    public DateTimeOffset EndUtc { get; }
    public AppointmentStatus Status { get; }

    public static Appointment Create(
        Guid id,
        Guid tenantId,
        Guid staffId,
        DateTimeOffset start,
        DateTimeOffset end,
        AppointmentStatus status = AppointmentStatus.Pending)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Appointment ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (staffId == Guid.Empty)
            throw new ArgumentException("Staff ID cannot be empty.", nameof(staffId));
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));

        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        if (endUtc <= startUtc)
            throw new ArgumentException("End must be strictly after start.", nameof(end));

        return new Appointment(id, tenantId, staffId, startUtc, endUtc, status);
    }

    public Appointment Cancel() =>
        new(Id, TenantId, StaffId, StartUtc, EndUtc, AppointmentStatus.Cancelled);

    public Appointment Reschedule(DateTimeOffset newStart, DateTimeOffset newEnd)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointments cannot be rescheduled.");

        return Create(Id, TenantId, StaffId, newStart, newEnd, Status);
    }
}
