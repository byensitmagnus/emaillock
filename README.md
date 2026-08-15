# EmailLock

Locks Outlook outside your working hours.

It's Sunday morning. You sit down to play something, and your hand opens Outlook out of
pure habit. Instead of your inbox, the screen goes dark:

> **Det er søndag, Magnus.**
> *Hvorfor vil du åbne den? Du skal ikke åbne den.*
>
> Åbner igen om **18t 46m**
>
> [ Luk og gå ]
> SOS — jeg har brug for den alligevel

Outlook closes. The countdown tells you exactly how long you have off. If you genuinely
need it, SOS asks for your code and unlocks everything for an hour.

On a weekday between 07:00 and 17:00, nothing happens at all.

**This is friction, not a prison.** Task Manager still wins. The point is to break the
reflex, not to defeat a determined you.

## Install

Download `EmailLock-x.y.z-setup.exe` from Releases and run it.

- No admin rights, no UAC prompt — it installs for your user only.
- No .NET to install: the runtime is bundled.
- Needs Microsoft Edge WebView2. Windows 11 already has it; on Windows 10 the installer
  fetches it for you.

The installer offers **Start EmailLock when Windows starts**. Leave it ticked — without it
the lock is gone after your next restart.

Uninstall from Settings → Apps like anything else. Your config file is left alone.

## Using it

EmailLock lives in the tray as an amber padlock. Double-click it, or right-click →
**Settings**, to open the window:

| Setting | What it does |
|---|---|
| **Name** | Goes into the message on the lock screen. |
| **SOS code** | What you type to unlock. Pick something you *cannot* type in your sleep — your birth date is ten keystrokes you already know, which is barely any friction at all. |
| **Locked days** | Locked around the clock. Defaults to Saturday and Sunday. |
| **Open hours** | On every other day, only this window is open. Everything outside it — evening, night, early morning — is locked. |
| **Apps** | Process names without `.exe`. `OUTLOOK` is classic Outlook, `olk` is new Outlook. Add `Thunderbird`, `ms-teams`, `slack` — whatever pulls you back in. |
| **SOS unlocks for** | Minutes before it locks itself again. |
| **Grace before force-close** | Outlook is asked to close politely first so it can prompt to save drafts. It is force-closed only after this many seconds. |
| **Message** | First line is the headline, the rest is the subtitle. `{day}`, `{time}` and `{owner}` are substituted. |

The window speaks Danish on a Danish Windows and English everywhere else.

Settings are stored in `%APPDATA%\EmailLock\config.json`. You can edit it by hand; anything
unusable in there makes the app **stay locked** and open Settings to tell you what's wrong.

**Quit is disabled during a locked hour.** During open hours you can quit freely from the
tray menu.

## How it works

A tray app checks once a second whether you're inside a locked window. If you are, and one
of your listed apps is running, it asks that app to close and shows the lock screen.

Nothing is installed into Outlook. No registry hooks into other processes, no admin rights,
no driver, no service. Both screens are HTML rendered in WebView2, so the design lives in
[`src/EmailLock/ui/`](src/EmailLock/ui/) rather than in hand-placed form controls.

### Known ceilings

- **It polls once a second.** Outlook can flash visible for up to a second before closing.
  The nag is the point, not the milliseconds.
- **It force-closes after the grace period.** Outlook autosaves drafts and gets a proper
  close request first, but an unanswered "save changes?" prompt will lose the last edits.
  Raise the grace period if that ever bites you.
- **Webmail in a browser is not covered.** Locking `OUTLOOK.EXE` does nothing about
  `outlook.office.com` in a tab. See [docs/roadmap.md](docs/roadmap.md).
- **Task Manager beats it.** By design.

## Building

Needs the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and, for the
installer, [Inno Setup 6](https://jrsoftware.org/isinfo.php)
(`winget install JRSoftware.InnoSetup`).

```bash
./build.ps1
```

Runs the tests, publishes self-contained, and writes `dist/EmailLock-x.y.z-setup.exe`.
It refuses to build a release from a red test run. `./build.ps1 -SkipInstaller` builds just
the app.

```bash
dotnet test
```

The schedule maths, the config validation and the SOS comparison are covered by tests and
take no dependency on the machine's clock or language — `now` is always passed in, and
rejected settings are named rather than worded so the assertions survive translation.

```
src/EmailLock/       app — Schedule.cs and Config.cs are the pure logic
src/EmailLock/ui/    the two screens, as HTML
tests/               xUnit
installer/           Inno Setup script
```

## Contributing

Issues and pull requests welcome. Two house rules:

1. New logic arrives with a test that failed first.
2. Keep the lock honest — anything that can't be parsed or understood must fail *closed*.

## License

MIT.
