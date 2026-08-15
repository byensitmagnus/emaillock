# Changelog

Notable changes, newest first. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/);
versions follow [semver](https://semver.org/).

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

[2.0.0]: https://github.com/byensitmagnus/emaillock/releases/tag/v2.0.0
