using Xunit;

namespace EmailLock.Tests;

// Calendar anchor for every test below (2026-08):
//   15 Sat | 16 Sun | 17 Mon | 19 Wed | 20 Thu | 21 Fri | 24 Mon
public class IsLockedTests
{
    static readonly Config Default = new();

    [Fact] public void Locked_day_is_locked_at_midday() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 15, 12, 0, 0), Default));

    [Fact] public void Locked_day_is_locked_during_office_hours_too() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 16, 9, 0, 0), Default));

    [Fact] public void Workday_midday_is_open() =>
        Assert.False(Schedule.IsLocked(new DateTime(2026, 8, 19, 12, 0, 0), Default));

    [Fact] public void Workday_one_minute_before_opening_is_locked() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 19, 6, 59, 0), Default));

    [Fact] public void Workday_opens_exactly_at_open_from() =>
        Assert.False(Schedule.IsLocked(new DateTime(2026, 8, 19, 7, 0, 0), Default));

    [Fact] public void Workday_last_minute_before_closing_is_open() =>
        Assert.False(Schedule.IsLocked(new DateTime(2026, 8, 19, 16, 59, 0), Default));

    [Fact] public void Workday_locks_exactly_at_open_until() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 19, 17, 0, 0), Default));

    [Fact] public void Workday_evening_is_locked() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 21, 22, 0, 0), Default));

    [Fact] public void Small_hours_of_a_workday_are_locked() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 17, 2, 0, 0), Default));

    // A hand-edited config must never be able to disable the lock, and must never throw
    // inside the once-a-second tick. Anything unusable fails closed.
    [Fact] public void Unreadable_times_fail_closed() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 19, 12, 0, 0), new Config { OpenFrom = "garbage" }));

    [Fact] public void Backwards_open_window_fails_closed() =>
        Assert.True(Schedule.IsLocked(new DateTime(2026, 8, 19, 12, 0, 0),
                                      new Config { OpenFrom = "17:00", OpenUntil = "07:00" }));
}

public class NextUnlockTests
{
    static readonly Config Default = new();

    [Fact]
    public void Saturday_waits_for_Monday_morning() =>
        Assert.Equal(new DateTime(2026, 8, 17, 7, 0, 0),
                     Schedule.NextUnlock(new DateTime(2026, 8, 15, 10, 0, 0), Default));

    [Fact]
    public void Late_Sunday_waits_for_Monday_morning() =>
        Assert.Equal(new DateTime(2026, 8, 17, 7, 0, 0),
                     Schedule.NextUnlock(new DateTime(2026, 8, 16, 23, 0, 0), Default));

    [Fact]
    public void Before_opening_waits_for_opening_the_same_day() =>
        Assert.Equal(new DateTime(2026, 8, 19, 7, 0, 0),
                     Schedule.NextUnlock(new DateTime(2026, 8, 19, 5, 0, 0), Default));

    [Fact]
    public void After_closing_waits_for_the_next_morning() =>
        Assert.Equal(new DateTime(2026, 8, 20, 7, 0, 0),
                     Schedule.NextUnlock(new DateTime(2026, 8, 19, 18, 0, 0), Default));

    [Fact]
    public void Friday_evening_skips_the_whole_weekend() =>
        Assert.Equal(new DateTime(2026, 8, 24, 7, 0, 0),
                     Schedule.NextUnlock(new DateTime(2026, 8, 21, 18, 0, 0), Default));

    [Fact]
    public void Already_open_unlocks_now()
    {
        var now = new DateTime(2026, 8, 19, 12, 0, 0);
        Assert.Equal(now, Schedule.NextUnlock(now, Default));
    }

    [Fact]
    public void Every_day_locked_never_unlocks()
    {
        var never = new Config { LockedDays = Enum.GetNames<DayOfWeek>() };
        Assert.Null(Schedule.NextUnlock(new DateTime(2026, 8, 19, 12, 0, 0), never));
    }
}
