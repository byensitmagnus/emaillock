# Changelog

Notable changes, newest first. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/);
versions follow [semver](https://semver.org/).

## [2.0.1] — 2026-09-25

### Fixed

- **First run no longer locks straight away.** Settings opens and nothing is enforced until
  you have saved once, with an SOS code of your own. Before, installing outside your open
  hours closed Outlook within 20 seconds behind a default code you had never seen.
- **The SOS code no longer reaches the Settings page**, and Settings can't be saved during a
  locked hour (use SOS first, as with *Quit*). Before, Settings showed the code and could
  untick today to unlock.
- **"Close and walk away" now stays out of the way for the grace period**, so the app's own
  *"save changes?"* prompt can be answered instead of hiding behind the lock screen.
- Only apps in your own Windows session are closed.
- A `config.json` that can't be read, or a day name the schedule doesn't know, now keeps
  you locked and says why in Settings, instead of silently falling back to the defaults.
- Settings no longer shows "Locked right now" during an SOS unlock.

## [2.0.0] — 2026-08-15

First public release.

### Added

- **Full-screen lock screen** with a live countdown to the moment your off-hours end.
- **SOS unlock** in the lock screen itself — type your code to unlock every listed app for
  a configurable number of minutes.
- **Settings window** for locked days, open hours, apps, SOS code, grace period, the lock
  screen message, and *Start with Windows*.
- **Danish and English**, chosen automatically from the Windows display language.
- **Per-user installer** — no admin rights, no UAC prompt, .NET bundled, and Microsoft Edge
  WebView2 fetched only if the machine doesn't already have it.
- **Config validation** with an explanation of exactly what is wrong, shown in the Settings
  window rather than swallowed.

### Changed

- Both screens are now HTML rendered in WebView2 instead of hand-placed WinForms controls,
  so the design lives in `src/EmailLock/ui/`.
- `Schedule` and `Config` are pure — `now` is passed in — and covered by 37 tests that
  depend on neither the clock nor the machine's language.

### Security

- A config the app cannot parse or make sense of now **fails closed**: you stay locked. An
  empty SOS code is rejected outright, so a cancelled prompt can no longer unlock.

[2.0.1]: https://github.com/byensitmagnus/emaillock/releases/tag/v2.0.1
[2.0.0]: https://github.com/byensitmagnus/emaillock/releases/tag/v2.0.0
