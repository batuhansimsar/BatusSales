namespace BatusSales.Api.Domain;

/// <summary>
/// Represents a half-open time interval [start, end).
/// Adjacent bookings are not overlapping; all comparisons normalize to UTC.
/// This rule alone does NOT prevent races; durable booking writes will
/// additionally require transactional database-level overlap protection.
/// </summary>
public sealed class BookingWindow
{
    public DateTimeOffset StartUtc { get; }
    public DateTimeOffset EndUtc { get; }

    public BookingWindow(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException("Booking end must be after start.", nameof(end));

        StartUtc = start.ToUniversalTime();
        EndUtc = end.ToUniversalTime();
    }

    public bool Overlaps(BookingWindow other) =>
        StartUtc < other.EndUtc && other.StartUtc < EndUtc;
}
