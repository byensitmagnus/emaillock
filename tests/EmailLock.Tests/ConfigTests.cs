using Xunit;

namespace EmailLock.Tests;

public class ConfigValidateTests
{
    // Assertions name the problem rather than quote it, so these pass on a Danish
    // machine and an English one alike.
    static void AssertRejects(string key, Config c) =>
        Assert.Contains(c.Validate(), p => p.Key == key);

    [Fact]
    public void Default_config_is_valid() =>
        Assert.Empty(new Config().Validate());

    [Fact]
    public void Rejects_a_time_that_is_not_a_clock_time() =>
        AssertRejects("badOpenFrom", new Config { OpenFrom = "25:00" });

    [Fact]
    public void Rejects_garbage_in_open_until() =>
        AssertRejects("badOpenUntil", new Config { OpenUntil = "half past five" });

    [Fact]
    public void Rejects_a_closing_time_before_the_opening_time() =>
        AssertRejects("backwardsWindow", new Config { OpenFrom = "17:00", OpenUntil = "07:00" });

    // An empty code would make the SOS prompt unlock on an empty answer -- i.e. on Cancel.
    [Fact]
    public void Rejects_an_empty_sos_code() =>
        AssertRejects("emptySosCode", new Config { SosCode = "   " });

    [Fact]
    public void Rejects_a_day_name_it_does_not_recognise() =>
        AssertRejects("unknownDay", new Config { LockedDays = new[] { "Lørdag" } });

    [Fact]
    public void Names_the_day_it_rejected_so_the_message_can_quote_it() =>
        Assert.Contains(new Config { LockedDays = new[] { "Lørdag" } }.Validate(),
                        p => p.Arg == "Lørdag");

    [Fact]
    public void Rejects_an_empty_app_list() =>
        AssertRejects("noApps", new Config { Apps = Array.Empty<string>() });

    [Fact]
    public void Rejects_an_sos_window_of_zero_minutes() =>
        AssertRejects("badSosMinutes", new Config { SosMinutes = 0 });

    [Fact]
    public void Rejects_a_negative_grace_period() =>
        AssertRejects("negativeGrace", new Config { GraceSeconds = -1 });

    [Fact]
    public void Accepts_an_empty_locked_days_list_as_a_weekday_only_schedule() =>
        Assert.Empty(new Config { LockedDays = Array.Empty<string>() }.Validate());

    // Every key Validate can emit must have prose in both languages, or a real problem
    // would surface to the user as a bare identifier.
    [Fact]
    public void Every_problem_key_has_text_in_every_language()
    {
        string[] keys = ["badOpenFrom", "badOpenUntil", "backwardsWindow", "emptySosCode",
                         "unknownDay", "noApps", "badSosMinutes", "negativeGrace"];
        foreach (var key in keys)
        {
            Assert.True(Strings.Da.ContainsKey("problem_" + key), $"Danish text missing for {key}");
            Assert.True(Strings.En.ContainsKey("problem_" + key), $"English text missing for {key}");
        }
    }
}
