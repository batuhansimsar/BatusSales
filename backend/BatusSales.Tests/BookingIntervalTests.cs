using BatusSales.Core.Bookings;
using Xunit;

namespace BatusSales.Tests;

public class BookingIntervalTests
{
    [Fact]
    public void AdjoiningAppointmentsDoNotOverlap()
    {
        var start = new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);
        var first = BookingInterval.Create(start, start.AddMinutes(30));
        var second = BookingInterval.Create(start.AddMinutes(30), start.AddMinutes(60));

        Assert.False(first.Overlaps(second));
        Assert.False(second.Overlaps(first));
    }

    [Fact]
    public void ConcurrentAppointmentsOverlap()
    {
        var start = new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);
        var first = BookingInterval.Create(start, start.AddMinutes(30));
        var second = BookingInterval.Create(start.AddMinutes(15), start.AddMinutes(45));

        Assert.True(first.Overlaps(second));
        Assert.True(second.Overlaps(first));
    }

    [Fact]
    public void InvalidTimeRangesAreRejected()
    {
        var start = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => BookingInterval.Create(start, start));
        Assert.Throws<ArgumentException>(() => BookingInterval.Create(start, start.AddMinutes(-1)));
    }

    [Fact]
    public void OffsetsAreNormalizedBeforeComparison()
    {
        var utc = new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);
        var local = new DateTimeOffset(2026, 10, 21, 13, 15, 0, TimeSpan.FromHours(3));
        var first = BookingInterval.Create(utc, utc.AddHours(1));
        var second = BookingInterval.Create(local, local.AddMinutes(15));

        Assert.True(first.Overlaps(second));
        Assert.Equal(TimeSpan.Zero, second.StartsAtUtc.Offset);
    }
}
