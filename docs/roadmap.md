# Roadmap — the founder off-switch

EmailLock is one tool. The idea it came from is bigger: a small set of things that make it
physically harder to work when you shouldn't be working.

Nothing below is decided. This file exists so the ideas stop living in a chat log.

## The shape

**One repo, one tray app.** EmailLock already runs 24/7 in the tray, so everything here is
20–40 lines on top of it rather than a second binary to install, update and explain.

## Candidates, ranked by value per line

### 1. Browser block for webmail — the real hole

Locking `OUTLOOK.EXE` does nothing about `outlook.office.com` in a Chrome tab. Right now
that makes the whole lock a five-second detour.

Approach: rewrite the `hosts` file during locked hours, restore it after. Works in every
browser at once, no extension to install per browser, no per-browser maintenance.
Needs admin to write `hosts` — that is the real cost, and the reason it is not done yet.

### 2. Streak counter in the tray

"Three weekends without opening Outlook." The lock already knows every time it fires and
every time SOS is used; showing it back is cheap.

Worth doing because invisible progress does not register. Visible progress does.

### 3. The 17:00 ritual

One prompt when the lock closes for the day: *three things you finished, one thing for
Monday.* Writes to a file.

Rumination over unfinished work is the actual mechanism behind checking mail on a Saturday —
not the mail. This is the only item here that addresses the cause rather than the symptom.

### 4. Notification curfew — do not build

Windows 11 already does this: Settings → System → Notifications → Do not disturb →
automatic rules. Document it in the README instead of reimplementing it.

### 5. Calendar and phone — later

Auto-inserting focus blocks needs Graph API auth; the phone needs a mobile app. Both are out
of proportion to the rest.

## The holes worth remembering

- **A public repo is not stress relief — it is a second job with strangers.** Issues, PR
  reviews, "doesn't work on my machine". A tool meant to remove work must not add work.
  Consider shipping with Issues disabled for the first month and a README that says *fork it*.
- **The tool removes your view of the mail, not the mail.** In a two-person company nobody
  else is covering the weekend inbox. Monday gets worse, not better, unless someone actually
  takes it or an autoresponder sets expectations.
- **A birth date is not friction.** Ten keystrokes you already know by heart. Real friction
  is a twenty-character random string printed and left in a drawer. The field already
  supports it; it is a choice, not a code change.
