using BatusSales.Api.Domain;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class BookingWindowTests
{
    [Fact]
    public void Adjacent_intervals_do_not_overlap()
    {
        var first = new BookingWindow(
            new DateTimeOffset(2026, 10, 21, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero));
        var next = new BookingWindow(
            new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 21, 11, 0, 0, TimeSpan.Zero));

        Assert.False(first.Overlaps(next));
        Assert.False(next.Overlaps(first));
    }

    [Fact]
    public void Partial_overlap_is_symmetric_even_with_different_offsets()
    {
        var first = new BookingWindow(
            new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.FromHours(3)),
            new DateTimeOffset(2026, 10, 21, 13, 0, 0, TimeSpan.FromHours(3)));
        var second = new BookingWindow(
            new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 21, 10, 30, 0, TimeSpan.Zero));

        Assert.True(first.Overlaps(second));
        Assert.True(second.Overlaps(first));
    }

    [Fact]
    public void Zero_or_negative_duration_is_rejected()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => new BookingWindow(now, now));
        Assert.Throws<ArgumentException>(() => new BookingWindow(now, now.AddMinutes(-1)));
    }
}
