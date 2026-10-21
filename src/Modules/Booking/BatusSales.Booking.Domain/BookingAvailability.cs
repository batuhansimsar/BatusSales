namespace BatusSales.Booking.Domain;

/// <summary>
/// In-memory domain preflight only. It is NOT a concurrency lock. The database must
/// enforce the same no-overlap rule transactionally before an appointment is persisted.
/// </summary>
public static class BookingAvailability
{
    public static bool IsAvailable(Appointment proposed, IEnumerable<Appointment> existing)
    {
        ArgumentNullException.ThrowIfNull(proposed);
        ArgumentNullException.ThrowIfNull(existing);

        if (proposed.Status == AppointmentStatus.Cancelled)
            throw new ArgumentException("A cancelled appointment cannot reserve a slot.", nameof(proposed));

        foreach (var current in existing)
        {
            if (current.TenantId != proposed.TenantId ||
                current.StaffId != proposed.StaffId ||
                current.Id == proposed.Id ||
                current.Status == AppointmentStatus.Cancelled)
                continue;

            // Half-open intervals [start, end): touching appointments do not overlap.
            if (current.StartUtc < proposed.EndUtc && proposed.StartUtc < current.EndUtc)
                return false;
        }

        return true;
    }
}
