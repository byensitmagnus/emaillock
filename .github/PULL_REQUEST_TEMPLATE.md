<!-- Thanks for this. Keep it short; the checklist matters more than the prose. -->

## What this changes

<!-- One or two sentences. If it fixes an issue, link it. -->

## Why

<!-- The problem, not the patch. -->

## Checklist

- [ ] `dotnet test` passes
- [ ] New logic has a test that failed before the fix
- [ ] Nothing new can leave the machine — no network calls, no telemetry, no accounts
- [ ] An unreadable or nonsensical config still fails **closed** (locked)
- [ ] No new dependency, or the PR says why one was unavoidable
- [ ] UI strings added to **both** `En` and `Da` in `Strings.cs`

## Screenshots

<!-- Required for anything that changes the lock screen or Settings. -->
