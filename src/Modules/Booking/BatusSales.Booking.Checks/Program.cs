using BatusSales.Booking.Domain;

var tenant = Guid.NewGuid();
var otherTenant = Guid.NewGuid();
var staff = Guid.NewGuid();
var otherStaff = Guid.NewGuid();
var start = new DateTimeOffset(2026, 10, 22, 9, 0, 0, TimeSpan.Zero);
var end = start.AddMinutes(30);
var original = Appointment.Create(Guid.NewGuid(), tenant, staff, start, end);

var checks = new (string Name, Action Run)[]
{
    ("equal slots conflict", () =>
        Assert(!BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), tenant, staff, start, end), [original]))),
    ("partially overlapping slots conflict", () =>
        Assert(!BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), tenant, staff, start.AddMinutes(15), end.AddMinutes(15)), [original]))),
    ("touching slots do not overlap", () =>
        Assert(BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), tenant, staff, end, end.AddMinutes(30)), [original]))),
    ("other tenant cannot block our staff", () =>
        Assert(BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), otherTenant, staff, start, end), [original]))),
    ("other staff is available", () =>
        Assert(BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), tenant, otherStaff, start, end), [original]))),
    ("cancelled slot does not block", () =>
        Assert(BookingAvailability.IsAvailable(
            Appointment.Create(Guid.NewGuid(), tenant, staff, start, end), [original.Cancel()]))),
    ("same appointment ID can be rescheduled", () =>
        Assert(BookingAvailability.IsAvailable(original.Reschedule(start.AddMinutes(5), end.AddMinutes(5)), [original]))),
    ("different timezone offsets are normalized", () =>
    {
        var local = Appointment.Create(Guid.NewGuid(), tenant, staff,
            new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.FromHours(3)),
            new DateTimeOffset(2026, 10, 22, 12, 30, 0, TimeSpan.FromHours(3)));
        Assert(!BookingAvailability.IsAvailable(local, [original]));
        Assert(local.StartUtc.Offset == TimeSpan.Zero);
    }),
    ("Istanbul business wall time maps to the intended UTC slot", () =>
    {
        var local = BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 10, 22, 12, 30, 0, DateTimeKind.Unspecified),
            "Europe/Istanbul");
        Assert(local.StartUtc == start && local.EndUtc == end);
        Assert(!BookingAvailability.IsAvailable(local, [original]));
    }),
    ("spring-forward missing wall time is rejected", () =>
        Throws<ArgumentException>(() => BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 3, 29, 2, 30, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 3, 29, 3, 30, 0, DateTimeKind.Unspecified),
            "Europe/Berlin"))),
    ("fall-back repeated wall time is rejected", () =>
        Throws<ArgumentException>(() => BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 10, 25, 2, 30, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 10, 25, 3, 30, 0, DateTimeKind.Unspecified),
            "Europe/Berlin"))),
    ("wall time rejects a pre-interpreted UTC DateTime", () =>
        Throws<ArgumentException>(() => BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 22, 12, 30, 0, DateTimeKind.Unspecified),
            "Europe/Istanbul"))),
    ("DST transition uses elapsed UTC time, not wall-clock subtraction", () =>
    {
        var acrossTransition = BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 10, 25, 1, 30, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 10, 25, 3, 30, 0, DateTimeKind.Unspecified),
            "Europe/Berlin");
        Assert(acrossTransition.EndUtc - acrossTransition.StartUtc == TimeSpan.FromHours(3));
    }),
    ("business local intervals must still have positive UTC duration", () =>
        Throws<ArgumentException>(() => BookingLocalTime.CreateAppointment(
            Guid.NewGuid(), tenant, staff,
            new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Unspecified),
            "Europe/Istanbul"))),
    ("empty tenant identifier is rejected", () =>
        Throws<ArgumentException>(() =>
            Appointment.Create(Guid.NewGuid(), Guid.Empty, staff, start, end))),
    ("zero or negative duration is rejected", () =>
        Throws<ArgumentException>(() =>
            Appointment.Create(Guid.NewGuid(), tenant, staff, start, start))),
    ("cancelled appointments cannot be rescheduled", () =>
        Throws<InvalidOperationException>(() =>
            original.Cancel().Reschedule(start.AddHours(1), end.AddHours(1)))),
    ("cancelled candidate cannot reserve a slot", () =>
        Throws<ArgumentException>(() =>
            BookingAvailability.IsAvailable(original.Cancel(), [])))
};

var failures = 0;
foreach (var (name, run) in checks)
{
    try
    {
        run();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception error)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {error.Message}");
    }
}

Console.WriteLine($"{checks.Length - failures}/{checks.Length} checks passed.");
return failures == 0 ? 0 : 1;

static void Assert(bool condition)
{
    if (!condition)
        throw new Exception("Assertion failed.");
}

static void Throws<T>(Action action) where T : Exception
{
    try
    {
        action();
    }
    catch (T)
    {
        return;
    }
    throw new Exception($"Expected {typeof(T).Name}.");
}
