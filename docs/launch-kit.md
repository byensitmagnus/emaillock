# Launch kit

For the maintainer. Not part of the README flow.

One message, everywhere: **Stop opening work when you're off.** Resist the urge to invent a
second slogan per channel.

Three rules that matter more than any of the drafts below:

- No bought stars, no vote rings, no sockpuppets. A small honest tool survives a slow launch;
  it does not survive being caught faking one.
- No numbers you can't prove. No "used by 500 founders", no download counts, no testimonials.
- Ask for feedback, not for installs. The post that reads like an ad gets removed; the post
  that reads like a person gets comments.

## Sequence

Do these in order. Each one makes the next look better.

1. **Publish the v2.0.0 release** with the installer attached.
2. **Set the social preview** — Settings → General → Social preview → upload
   `docs/assets/social-preview.png`. Without it, every shared link is a grey box.
3. **Download the installer from the release page on a different machine** and run it. If
   the first stranger to try it hits a broken download, nothing else matters.
4. **Check the README renders** in both GitHub themes, and that the GIF plays.
5. **Then** post. LinkedIn first — it's your own audience and the lowest risk. Reddit next.
   Hacker News last, on a weekday morning US time, once you've seen the questions the
   first two produced.

Expect nothing. Most launches are quiet. The tool still works.

---

## Hacker News

Title:

> Show HN: EmailLock – a Windows app that closes Outlook outside your working hours

Comment to post immediately after submitting:

> I run a small company, and I kept noticing that I'd decide "I'm not working today" and
> then find myself reading email twenty minutes later — having never actually decided to
> start working. The app was open before the thought was.
>
> So this is deliberately not a productivity tool. It's about four seconds of friction. On
> the days and hours you pick, it closes the apps you listed and puts a full-screen message
> up with a countdown to when you're allowed back. There's an SOS button that unlocks
> everything for an hour if you actually need it.
>
> Task Manager kills it in six seconds, and I'm not going to fix that. Making it unkillable
> means a service, a driver and admin rights, and at that point it's employee monitoring
> software with a different name. It's meant to interrupt the automatic version of you, not
> defeat the determined one.
>
> C#/.NET 8, WinForms tray with the two screens rendered as HTML in WebView2. No telemetry,
> no network calls, no accounts — the schedule and validation logic is pure and covered by
> tests so it doesn't depend on the clock or the machine's language. MIT.
>
> The obvious gap: it does nothing about webmail in a browser tab. That's the top roadmap
> item and I'd like opinions on whether rewriting the hosts file is too blunt an instrument.

Ending on a real open question tends to get better threads than ending on a pitch.

## Reddit

**One subreddit: [r/productivity](https://reddit.com/r/productivity).** It's where people
with this exact problem already are. Read their self-promotion rules first — most subs allow
a free open-source tool if you wrote it and you're asking for feedback, and remove it if it
reads like marketing. If it's declined, r/digitalminimalism is the closer thematic fit.

Title:

> I kept opening Outlook on Saturdays without deciding to. So I built something that closes it.

Body:

> Not a discipline problem. I'd genuinely decide I wasn't working, and then twenty minutes
> later notice I was reading email — I never made a decision to start. The reflex fired
> faster than the intention that was supposed to stop it.
>
> So I made a small Windows app. On the days and hours I pick, it closes the apps I listed
> and puts a countdown on screen showing exactly how long I've got off. There's an override
> if I actually need to work.
>
> The part I'm least sure about: I deliberately made it easy to bypass. Task Manager closes
> it in seconds. My reasoning is that anything harder stops being a nudge and starts being
> the kind of software companies install on employees. But I don't know if that makes it too
> weak to work long-term for other people.
>
> It's free and open source, Windows only, no accounts or tracking. Link in a comment so
> this doesn't read as an ad — mostly I want to know whether the "easy to bypass on purpose"
> call is right, or whether I'm just designing an off-switch I'll press.

Put the link in your own first comment, and answer everything for the first few hours.

## LinkedIn

> I run a company, and apparently I'm also the employee most likely to ignore my own
> working hours.
>
> A few weeks ago I decided I wasn't working that Saturday. I meant it. Twenty minutes
> later I was reading email — and I couldn't point to the moment I decided to start. My
> hand had opened Outlook before the thought caught up.
>
> That's not a discipline problem. It's a reflex, and it's faster than the intention that's
> supposed to stop it.
>
> So I built a small thing to interrupt it. On the days and hours I choose, it closes my
> work apps and puts one line on the screen: you're off, go be off. Plus a countdown to
> when I'm allowed back, and an override for when I genuinely need it.
>
> It takes about four seconds to get past. That's the entire design. Four seconds is enough
> to ask "do I actually need to work right now?", and the answer is usually no — I just
> needed someone to ask.
>
> It's free, open source and does nothing except close apps on a schedule. No accounts, no
> tracking, nothing to buy. If you're the kind of founder who keeps promising yourself a
> real day off, it's yours.
>
> Sometimes the person breaking your work-life boundaries is you.

Post it on a weekday morning. Don't add fifteen hashtags; two or three at most.

## GitHub repository settings

- **Description:** A Windows app that stops you opening work apps outside your chosen hours.
  Built for founders who are bad at taking days off.
- **Website:** leave empty until there's something better than the repo itself.
- **Topics:** `windows`, `productivity`, `work-life-balance`, `digital-wellbeing`, `focus`,
  `outlook`, `dotnet`, `open-source`
- **Social preview:** `docs/assets/social-preview.png`
- **Issues:** consider leaving them **off for the first month**. The README already says the
  project is best-effort and tells people to fork. You built this to work less; a launch is
  the easiest way to accidentally acquire a support queue.
