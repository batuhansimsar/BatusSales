namespace BatusSales.Core.Bookings;

/// <summary>
/// UTC half-open interval [start, end). Adjoining bookings do not overlap.
/// The database must also enforce conflict safety transactionally.
/// </summary>
public sealed record BookingInterval
{
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }

    private BookingInterval(DateTimeOffset start, DateTimeOffset end)
    {
        StartsAtUtc = start;
        EndsAtUtc = end;
    }

    public static BookingInterval Create(DateTimeOffset start, DateTimeOffset end)
    {
        var normalizedStart = start.ToUniversalTime();
        var normalizedEnd = end.ToUniversalTime();

        if (normalizedStart >= normalizedEnd)
        {
            throw new ArgumentException("Booking end must be after its start.", nameof(end));
        }

        return new BookingInterval(normalizedStart, normalizedEnd);
    }

    public bool Overlaps(BookingInterval other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return StartsAtUtc < other.EndsAtUtc && other.StartsAtUtc < EndsAtUtc;
    }
}
