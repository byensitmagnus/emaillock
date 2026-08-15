# Contributing

Pull requests and issues are welcome.

**This project is maintained on a best-effort basis. There is no support SLA.** EmailLock
exists to help its maintainer work less, so it will never grow a Discord, a newsletter or a
support inbox — those would turn a small tool into another job. If a PR sits for a while,
fork it. That's what the MIT licence is for.

## Principles

A change is much likelier to be merged if it respects these. They are also the reason most
feature requests get a "no", which is what keeps the app small enough to understand.

1. **Friction, not enforcement.** EmailLock interrupts an automatic reflex. It does not try
   to overpower a conscious decision, and it is never built for one person to monitor
   another.
2. **Local-first.** Nothing leaves the machine. No telemetry, no analytics, no accounts, no
   cloud.
3. **Minimal dependencies.** The app has exactly one NuGet package. Adding a second needs a
   reason a few lines of code can't cover.
4. **Fail closed.** Anything the app cannot parse or make sense of leaves the user *locked*.
   A bug must never be able to silently unlock the day.
5. **Meaningful logic arrives with a test that failed first.** See below.
6. **Simplicity beats feature count.** The best patch is usually the one that deletes
   something.

## Working on it

```powershell
dotnet test                 # 37 tests, no clock or language dependency
./build.ps1 -SkipInstaller  # build the app
./build.ps1                 # full release, needs Inno Setup 6
```

`build.ps1` refuses to package a failing test run.

### Where things live

`Schedule.cs` and `Config.cs` are pure logic — `now` is always passed in, so you can test
a Sunday evening on a Tuesday morning. That is where new behaviour should go whenever it
can, because it's the part that can be proven.

`ui/*.html` is the design. Both screens are HTML rendered in WebView2, so visual changes
need no C# at all.

### Two conventions that are easy to miss

- **Validation returns keys, not sentences.** `Config.Validate()` names the problem
  (`backwardsWindow`) and the UI turns it into prose. This keeps the tests passing on a
  Danish machine and an English one alike, so don't move wording back into `Validate`.
- **Every user-visible string exists twice**, in `En` and `Da` in `Strings.cs`. A test
  fails if a validation key is missing from either.

### Screenshots and assets

`docs/assets/` is generated, not hand-made:

```powershell
./tools/capture-assets.ps1      # screenshots and demo.gif from the real app (needs ffmpeg)
./tools/make-social-preview.ps1 # the repository card
```

`capture-assets.ps1` temporarily patches a throwaway build to force `en-US` and open
Settings on start, then restores the file. Capture uses `PrintWindow`, not a screen grab, so
nothing from the desktop behind the window can end up in a public image.

## Releasing

Bump the version in **both** `installer/EmailLock.iss` and `src/EmailLock/EmailLock.csproj`,
add a `CHANGELOG.md` entry, then push a matching tag:

```powershell
git tag v2.0.1 && git push origin v2.0.1
```

The release workflow verifies the tag against both files, runs the tests, builds the
installer and attaches it. It refuses to publish if any of those disagree.
