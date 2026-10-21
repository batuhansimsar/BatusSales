namespace BatusSales.Booking.Domain;

/// <summary>
/// Convert an explicitly entered business wall-clock interval into UTC instants.
/// An ambiguous local time has two possible instants, and an invalid one has none.
/// Reject both instead of silently scheduling at an unintended time.
/// </summary>
public static class BookingLocalTime
{
    public static Appointment CreateAppointment(
        Guid id,
        Guid tenantId,
        Guid staffId,
        DateTime localStart,
        DateTime localEnd,
        string timeZoneId,
        AppointmentStatus status = AppointmentStatus.Pending)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var startUtc = Resolve(localStart, zone, nameof(localStart));
        var endUtc = Resolve(localEnd, zone, nameof(localEnd));

        // The canonical Appointment invariant validates the actual elapsed
        // interval after conversion (which may differ across a DST boundary).
        return Appointment.Create(
            id, tenantId, staffId,
            new DateTimeOffset(startUtc, TimeSpan.Zero),
            new DateTimeOffset(endUtc, TimeSpan.Zero),
            status);
    }

    private static DateTime Resolve(DateTime local, TimeZoneInfo zone, string paramName)
    {
        // Unspecified denotes an entered wall-clock time. Local or Utc here
        // would apply the host's zone or risk a second conversion.
        if (local.Kind != DateTimeKind.Unspecified)
            throw new ArgumentException(
                "Business wall-clock input must have DateTimeKind.Unspecified.", paramName);

        if (zone.IsInvalidTime(local))
            throw new ArgumentException(
                "This local time does not exist in the business time zone (DST gap).", paramName);

        if (zone.IsAmbiguousTime(local))
            throw new ArgumentException(
                "This local time occurs twice in the business time zone (DST overlap).", paramName);

        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }
}
