using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Win32;

namespace EmailLock;

static class Program
{
    public const string AppName = "EmailLock";

    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName, "config.json");

    [STAThread]
    static void Main()
    {
        // A second copy would fight the first over closing the same process.
        using var mutex = new Mutex(true, @"Global\EmailLock.SingleInstance", out var isFirst);
        if (!isFirst) return;

        ApplicationConfiguration.Initialize();
        Application.Run(new App());
    }
}

class App : ApplicationContext
{
    Config _cfg;
    readonly NotifyIcon _tray;
    readonly ToolStripMenuItem _status = new("...") { Enabled = false };
    readonly ToolStripMenuItem _quit;

    readonly Dictionary<int, DateTime> _seen = new();
    DateTime _sosUntil = DateTime.MinValue;
    DateTime _quietUntil = DateTime.MinValue;
    WebWindow? _lock, _settings;

    // No config.json yet = first run. Nothing is enforced until Settings has been saved once,
    // or installing on a Saturday evening would close Outlook behind a code nobody has chosen.
    bool _configured = File.Exists(Program.ConfigPath);
    bool HasCode => _configured && _cfg.LoadError is null;

    static readonly int Session = Process.GetCurrentProcess().SessionId;

    bool LockedNow(DateTime now) => _configured && now >= _sosUntil && Schedule.IsLocked(now, _cfg);

    public App()
    {
        _cfg = Load();

        _quit = new ToolStripMenuItem(Strings.Get("trayQuit"));
        _quit.Click += (_, _) => ExitThread();

        var menu = new ContextMenuStrip();
        menu.Items.Add(_status);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.Get("traySettings"), null, async (_, _) => await ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_quit);
        menu.Opening += (_, _) => RefreshMenu();

        _tray = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Shield,
            Text = Program.AppName,
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += async (_, _) => await ShowSettings();

        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += Tick;
        timer.Start();

        // A first run, or a config the app cannot honour, is worth interrupting for, once, at startup.
        if (!_configured || _cfg.Validate().Count > 0) _ = ShowSettings();
    }

    // --- the loop ---------------------------------------------------------

    void Tick(object? sender, EventArgs e)
    {
        if (!LockedNow(DateTime.Now))
        {
            _seen.Clear();
            return;
        }

        // Only this Windows session: another user's Outlook is not ours to close, and could not be.
        var running = _cfg.Apps
            .SelectMany(name => { try { return Process.GetProcessesByName(name); } catch { return []; } })
            .Where(p => { try { return p.SessionId == Session; } catch { return false; } })
            .ToList();

        if (running.Count == 0)
        {
            _seen.Clear();
            return;
        }

        if (DateTime.Now >= _quietUntil) _ = ShowLock();
        foreach (var p in running) Shoo(p);
    }

    void Shoo(Process p)
    {
        try
        {
            if (!_seen.TryGetValue(p.Id, out var since)) _seen[p.Id] = since = DateTime.Now;

            // ponytail: ask nicely first so Outlook can prompt to save, hard close after graceSeconds.
            // Ceiling: an unanswered save prompt loses the last edits. Raise graceSeconds if that bites.
            if ((DateTime.Now - since).TotalSeconds > _cfg.GraceSeconds) p.Kill();
            else p.CloseMainWindow();
        }
        catch { /* it died between the scan and here -- that was the goal */ }
    }

    // --- lock screen ------------------------------------------------------

    async Task ShowLock()
    {
        if (_lock is { IsDisposed: false }) return;

        var w = new WebWindow("lock.html", LockData());
        _lock = w;
        w.FillPrimaryScreen();
        w.MessageReceived += m =>
        {
            switch (m.GetProperty("type").GetString())
            {
                case "dismiss":
                    // "Close and walk away": stay out of the way for the grace period, or the
                    // lock screen would bury the app's own "save changes?" prompt until the force-close.
                    _quietUntil = DateTime.Now.AddSeconds(_cfg.GraceSeconds);
                    w.Close();
                    break;
                case "expired":
                    w.Close();
                    break;
                case "sos":
                    if (_cfg.AcceptsSosCode(m.GetProperty("code").GetString()))
                    {
                        _sosUntil = DateTime.Now.AddMinutes(_cfg.SosMinutes);
                        _seen.Clear();
                        w.Close();
                        _tray.ShowBalloonTip(3000, Program.AppName,
                            string.Format(Strings.Get("unlockedUntil"), _sosUntil.ToString("HH:mm")), ToolTipIcon.None);
                    }
                    else _ = w.Call("window.sosRejected()");
                    break;
            }
        };
        w.FormClosed += (_, _) => _lock = null;

        await w.PrepareAsync();
        w.Show();
        w.Activate();
    }

    object LockData()
    {
        var now = DateTime.Now;
        var text = Render(_cfg.Message, now);
        var parts = text.Split('\n', 2);

        var t = new Dictionary<string, string>(Strings.All)
        {
            ["sosHint"] = string.Format(Strings.Get("sosHint"), _cfg.SosMinutes)
        };

        return new
        {
            t,
            headline = parts[0].Trim(),
            subline = parts.Length > 1 ? parts[1].Trim() : "",
            nextUnlock = Schedule.NextUnlock(now, _cfg)?.ToString("s")
        };
    }

    string Render(string template, DateTime now) => template
        .Replace("{owner}", _cfg.Owner)
        .Replace("{day}", DayName(now))
        .Replace("{time}", now.ToString("HH:mm"));

    // Left exactly as the culture writes it: Danish keeps weekdays lowercase mid-sentence,
    // English capitalises them. Title-casing here would be wrong in one of the two.
    static string DayName(DateTime d) =>
        CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d.DayOfWeek);

    // --- settings ---------------------------------------------------------

    async Task ShowSettings()
    {
        if (_settings is { IsDisposed: false }) { _settings.Activate(); return; }

        var w = new WebWindow("settings.html", SettingsData());
        _settings = w;
        w.Panel(880, 700);
        w.MessageReceived += m =>
        {
            switch (m.GetProperty("type").GetString())
            {
                case "close": w.Close(); break;
                case "minimise": w.WindowState = FormWindowState.Minimized; break;
                case "drag": w.BeginDrag(); break;
                case "save": Save(m, w); break;
            }
        };
        w.FormClosed += (_, _) => _settings = null;

        await w.PrepareAsync();
        w.Show();
        w.Activate();
    }

    void Save(JsonElement m, WebWindow w)
    {
        var incoming = m.GetProperty("config").Deserialize<Config>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Config();

        // No saving your way out of a locked hour either -- SOS first. A config the app can't
        // honour is the exception: it already locks everything, and it has to stay fixable.
        if (LockedNow(DateTime.Now) && _cfg.Validate().Count == 0)
        {
            _ = w.Call($"window.saveResult({JsonSerializer.Serialize(new[] { Strings.Get("saveLocked") })},null)");
            return;
        }

        // The page never had the code (see Config.ForPage): a blank field keeps the saved one.
        if (HasCode && string.IsNullOrWhiteSpace(incoming.SosCode)) incoming.SosCode = _cfg.SosCode;

        var problems = incoming.Validate();
        if (problems.Count > 0)
        {
            _ = w.Call($"window.saveResult({JsonSerializer.Serialize(Describe(problems))},null)");
            return;
        }

        _cfg = incoming;
        Directory.CreateDirectory(Path.GetDirectoryName(Program.ConfigPath)!);
        File.WriteAllText(Program.ConfigPath,
            JsonSerializer.Serialize(_cfg, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        _configured = true;

        SetAutoStart(m.GetProperty("autostart").GetBoolean());
        _seen.Clear();

        // The saved schedule may have changed what "now" means -- refresh the status card.
        _ = w.Call($"window.saveResult([],{JsonSerializer.Serialize(Status(DateTime.Now), WebWindow.JsonOpts)})");
    }

    /// <summary>Turns named problems into the sentence the user actually reads.</summary>
    static string[] Describe(IReadOnlyList<Config.Problem> problems) =>
        problems.Select(p => string.Format(Strings.Get("problem_" + p.Key), p.Arg)).ToArray();

    object Status(DateTime now) => new
    {
        locked = LockedNow(now),
        lead = LockedNow(now) ? Strings.Get("stateLocked") : Strings.Get("stateOpen"),
        tail = StatusTail(now)
    };

    object SettingsData()
    {
        var now = DateTime.Now;
        var days = Enumerable.Range(0, 7).Select(i => (DayOfWeek)(((int)DayOfWeek.Monday + i) % 7)).ToArray();

        return new
        {
            t = Strings.All,
            config = _cfg.ForPage(),
            hasCode = HasCode,
            autostart = IsAutoStart(),
            locked = LockedNow(now),
            statusTail = StatusTail(now),
            // Non-empty when the window opened because the file on disk was unusable.
            problems = Describe(_cfg.Validate()),
            dayKeys = days.Select(d => d.ToString()).ToArray(),
            dayNames = days.Select(d => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d))).ToArray()
        };
    }

    string StatusTail(DateTime now)
    {
        if (!_configured) return Strings.Get("notActiveYet");

        if (now < _sosUntil)
            return string.Format(Strings.Get("unlockedUntil"), _sosUntil.ToString("HH:mm"));

        if (!Schedule.IsLocked(now, _cfg))
            return string.Format(Strings.Get("locksAt"), _cfg.OpenUntil);

        var next = Schedule.NextUnlock(now, _cfg);
        return next is null
            ? Strings.Get("opensNever")
            : string.Format(Strings.Get("opensAt"),
                next.Value.Date == now.Date ? next.Value.ToString("HH:mm") : $"{DayName(next.Value)} {next.Value:HH:mm}");
    }

    // --- tray -------------------------------------------------------------

    void RefreshMenu()
    {
        var now = DateTime.Now;
        var locked = LockedNow(now);
        _status.Text = (locked ? Strings.Get("stateLocked") : Strings.Get("stateOpen")) + " · " + StatusTail(now);
        // No quitting your way out of a locked hour. During open hours, quit freely.
        _quit.Enabled = !locked;
    }

    // --- config + autostart ------------------------------------------------

    static Config Load()
    {
        try
        {
            if (File.Exists(Program.ConfigPath))
                return JsonSerializer.Deserialize<Config>(File.ReadAllText(Program.ConfigPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Fresh();
        }
        catch (Exception e)
        {
            // Unreadable config must not unlock anything. The defaults alone would -- they are a
            // valid schedule -- so flag it: Schedule fails closed and Settings says why.
            var c = Fresh();
            c.LoadError = e.Message;
            return c;
        }
        return Fresh();
    }

    static Config Fresh() => new() { Message = Strings.Get("defaultMessage") };

    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    static bool IsAutoStart() => Registry.CurrentUser.OpenSubKey(RunKey)?.GetValue(Program.AppName) != null;

    static void SetAutoStart(bool on)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)!;
        if (on) key.SetValue(Program.AppName, $"\"{Environment.ProcessPath}\"");
        else key.DeleteValue(Program.AppName, throwOnMissingValue: false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _tray.Dispose();
        base.Dispose(disposing);
    }
}
