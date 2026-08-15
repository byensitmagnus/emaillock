namespace EmailLock;

/// <summary>
/// User settings, loaded from %APPDATA%\EmailLock\config.json.
/// Every field has a working default so a missing or partial file still produces a locked app.
/// </summary>
public class Config
{
    public string Owner { get; set; } = "you";
    public string SosCode { get; set; } = "1990-01-01";
    public string[] Apps { get; set; } = { "OUTLOOK", "olk" };
    public string[] LockedDays { get; set; } = { "Saturday", "Sunday" };
    public string OpenFrom { get; set; } = "07:00";
    public string OpenUntil { get; set; } = "17:00";
    public int SosMinutes { get; set; } = 60;
    public int GraceSeconds { get; set; } = 20;
    public string Message { get; set; } =
        "It's {day}, {owner}.\nWhy do you want to open this?";

    /// <summary>
    /// Whether a typed SOS answer should unlock. The comparison lives here rather than in
    /// the page so the code never reaches the browser, and so it can be tested directly.
    /// </summary>
    public bool AcceptsSosCode(string? typed) =>
        !string.IsNullOrWhiteSpace(SosCode) &&
        string.Equals(typed?.Trim(), SosCode.Trim(), StringComparison.Ordinal);

    /// <summary>
    /// A rejected setting, named rather than worded. Keeping the prose out of here means
    /// the message can be translated and the tests stay independent of the machine's language.
    /// </summary>
    public readonly record struct Problem(string Key, string Arg = "");

    /// <summary>
    /// One <see cref="Problem"/> per setting that would misbehave at runtime.
    /// Empty means the config is safe to run.
    /// </summary>
    public IReadOnlyList<Problem> Validate()
    {
        var problems = new List<Problem>();

        var fromOk = Schedule.TryTime(OpenFrom, out var from);
        var untilOk = Schedule.TryTime(OpenUntil, out var until);

        if (!fromOk) problems.Add(new("badOpenFrom", OpenFrom));
        if (!untilOk) problems.Add(new("badOpenUntil", OpenUntil));
        if (fromOk && untilOk && from >= until)
            problems.Add(new("backwardsWindow", $"{OpenFrom}–{OpenUntil}"));

        // An empty code makes the SOS prompt accept an empty answer -- Cancel would unlock.
        if (string.IsNullOrWhiteSpace(SosCode))
            problems.Add(new("emptySosCode"));

        foreach (var day in LockedDays)
            if (!Enum.TryParse<DayOfWeek>(day, ignoreCase: true, out _))
                problems.Add(new("unknownDay", day));

        if (Apps.Length == 0) problems.Add(new("noApps"));
        if (SosMinutes <= 0) problems.Add(new("badSosMinutes"));
        if (GraceSeconds < 0) problems.Add(new("negativeGrace"));

        return problems;
    }
}
