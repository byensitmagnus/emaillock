using System.Globalization;

namespace EmailLock;

/// <summary>
/// Pure schedule maths. No Windows, no clock of its own -- `now` is always passed in
/// so every branch is reachable from a test.
/// </summary>
public static class Schedule
{
    /// <summary>Days in <see cref="Config.LockedDays"/> are locked around the clock.
    /// Every other day is open only inside [OpenFrom, OpenUntil).
    /// A config this can't make sense of fails closed -- locked.</summary>
    public static bool IsLocked(DateTime now, Config c)
    {
        if (IsLockedDay(now, c)) return true;
        if (!TryWindow(c, out var from, out var until)) return true;

        var t = now.TimeOfDay;
        return t < from || t >= until;
    }

    /// <summary>The next instant <see cref="IsLocked"/> turns false, or null if it never does.
    /// Returns <paramref name="now"/> when already unlocked.</summary>
    public static DateTime? NextUnlock(DateTime now, Config c)
    {
        if (!TryWindow(c, out var from, out var until)) return null;

        // A week of lookahead is enough: any day-of-week that is ever open recurs within 7 days.
        for (var d = 0; d <= 7; d++)
        {
            var day = now.Date.AddDays(d);
            if (IsLockedDay(day, c)) continue;

            if (now < day + from) return day + from;
            if (now < day + until) return now;
        }
        return null;
    }

    internal static bool IsLockedDay(DateTime d, Config c) =>
        c.LockedDays.Any(x => x.Equals(d.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Open hours never cross midnight, so a backwards window is a broken window.</summary>
    static bool TryWindow(Config c, out TimeSpan from, out TimeSpan until)
    {
        until = default;
        return TryTime(c.OpenFrom, out from) && TryTime(c.OpenUntil, out until) && from < until;
    }

    internal static bool TryTime(string value, out TimeSpan time) =>
        TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out time)
        && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
}
