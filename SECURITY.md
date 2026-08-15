# Security

## What EmailLock actually touches

Worth knowing before you assess risk:

- It makes **no network requests**. There is no HTTP client in the application.
- It runs **without admin rights** and installs per-user.
- It has **no access to your mail** — no Outlook add-in, no COM registration, no
  credentials. It only sees that a process with a given name is running.
- It writes exactly two things outside its own install folder: `%APPDATA%\EmailLock\config.json`,
  and one `HKEY_CURRENT_USER\...\Run` entry if you enable *Start with Windows*.
- Its one privileged action is closing processes you listed yourself.

**Your SOS code is stored in plain text** in `config.json`. That is deliberate: it is
friction, not a password. It protects your own intentions, nothing else. Don't reuse a real
password there, and remove it before pasting the file into an issue.

## Reporting a vulnerability

Use [private vulnerability reporting](https://github.com/byensitmagnus/emaillock/security/advisories/new)
on this repository. Please don't open a public issue for anything that could be abused
before it's fixed.

Include what you'd expect: what you did, what happened, and why it matters.

**Expectations, honestly:** this is a small tool maintained in spare time. There is no
bounty, no SLA and no dedicated security team. Genuine issues will be taken seriously and
fixed as quickly as one person reasonably can. Everything else may take a while.

## Supported versions

The latest release only. There are no backports.

## Not vulnerabilities

These are documented design decisions, not security holes:

- **EmailLock can be closed from Task Manager.** [On purpose.](README.md#this-is-friction-not-a-prison)
  Bypassing the lock on your own machine is a feature, not an exploit.
- **The SOS code is readable in the config file.** See above.
- **The installer is unsigned**, so SmartScreen warns on first run. A signing certificate is
  a roadmap item, and the README says so plainly.
