using System.Globalization;

namespace EmailLock;

/// <summary>
/// UI text. Danish when Windows is Danish, English otherwise -- the whole point of the app
/// is that it speaks to you personally, and a half-translated screen breaks that.
/// </summary>
public static class Strings
{
    public static bool Danish =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("da", StringComparison.OrdinalIgnoreCase);

    public static Dictionary<string, string> All => Danish ? Da : En;

    public static string Get(string key) => All.TryGetValue(key, out var v) ? v : key;

    public static readonly Dictionary<string, string> En = new()
    {
        ["lockedNow"] = "LOCKED",
        ["opensIn"] = "Opens again in",
        ["opensNever"] = "No open hours in the schedule",
        ["dismiss"] = "Close and walk away",
        ["sos"] = "SOS — I need it anyway",
        ["sosTitle"] = "Enter your code",
        ["sosHint"] = "Unlocks everything for {0} minutes.",
        ["sosWrong"] = "Wrong. Still locked.",
        ["unlock"] = "Unlock",
        ["cancel"] = "Never mind",
        ["hour"] = "h",
        ["minute"] = "m",

        ["secCode"] = "YOU",
        ["owner"] = "Name",
        ["ownerNote"] = "Used in the message on the lock screen.",
        ["sosCode"] = "SOS code",
        ["sosCodeNote"] = "What you have to type to unlock. Pick something you cannot type in your sleep.",
        ["secDays"] = "LOCKED DAYS",
        ["secHours"] = "OPEN HOURS",
        ["hours"] = "Open between",
        ["hoursNote"] = "On days that are not locked. Everything outside the window is locked.",
        ["secApps"] = "APPS",
        ["appsNote"] = "Process names without .exe. OUTLOOK is classic Outlook, olk is new Outlook. Press Enter to add.",
        ["secMore"] = "DETAILS",
        ["sosMinutes"] = "SOS unlocks for",
        ["sosMinutesNote"] = "Minutes before it locks itself again.",
        ["grace"] = "Grace before force-close",
        ["graceNote"] = "Outlook is asked to close politely first, so it can prompt to save drafts.",
        ["autostart"] = "Start with Windows",
        ["autostartNote"] = "Without this the lock is gone after a restart.",
        ["secMsg"] = "MESSAGE",
        ["msgNote"] = "First line is the headline. {day}, {time} and {owner} are substituted.",
        ["save"] = "Save",
        ["savedOk"] = "Saved",
        ["addApp"] = "Add…",
        ["stateLocked"] = "Locked right now",
        ["stateOpen"] = "Open right now",
        ["opensAt"] = "Opens {0}",
        ["locksAt"] = "Locks at {0}",
        ["unlockedUntil"] = "Unlocked until {0}",
        ["traySettings"] = "Settings…",
        ["trayQuit"] = "Quit",
        ["quitPrompt"] = "Quitting removes the lock. Enter your code:",
        ["defaultMessage"] = "It's {day}, {owner}.\nWhy do you want to open this? You should not open it.",

        ["problem_badOpenFrom"] = "Open from: '{0}' is not a time of day. Use HH:mm, for example 07:00.",
        ["problem_badOpenUntil"] = "Open until: '{0}' is not a time of day. Use HH:mm, for example 17:00.",
        ["problem_backwardsWindow"] = "The open window {0} runs backwards. Open hours do not cross midnight.",
        ["problem_emptySosCode"] = "The SOS code is empty. SOS would then unlock on an empty answer.",
        ["problem_unknownDay"] = "'{0}' is not an English day name (Monday … Sunday).",
        ["problem_noApps"] = "The app list is empty. Nothing would ever be locked.",
        ["problem_badSosMinutes"] = "SOS must unlock for at least one minute.",
        ["problem_negativeGrace"] = "The grace period cannot be negative.",
    };

    public static readonly Dictionary<string, string> Da = new()
    {
        ["lockedNow"] = "LÅST",
        ["opensIn"] = "Åbner igen om",
        ["opensNever"] = "Ingen åbningstid i kalenderen",
        ["dismiss"] = "Luk og gå",
        ["sos"] = "SOS — jeg har brug for den alligevel",
        ["sosTitle"] = "Skriv din kode",
        ["sosHint"] = "Låser alt op i {0} minutter.",
        ["sosWrong"] = "Forkert. Stadig låst.",
        ["unlock"] = "Lås op",
        ["cancel"] = "Fortryd",
        ["hour"] = "t",
        ["minute"] = "m",

        ["secCode"] = "DIG",
        ["owner"] = "Navn",
        ["ownerNote"] = "Bruges i beskeden på låseskærmen.",
        ["sosCode"] = "SOS-kode",
        ["sosCodeNote"] = "Det du skal skrive for at låse op. Vælg noget du ikke kan i søvne.",
        ["secDays"] = "LÅSTE DAGE",
        ["secHours"] = "ÅBNINGSTID",
        ["hours"] = "Åben mellem",
        ["hoursNote"] = "På dage der ikke er låst. Alt uden for vinduet er låst.",
        ["secApps"] = "PROGRAMMER",
        ["appsNote"] = "Procesnavne uden .exe. OUTLOOK er klassisk Outlook, olk er nye Outlook. Tryk Enter for at tilføje.",
        ["secMore"] = "DETALJER",
        ["sosMinutes"] = "SOS låser op i",
        ["sosMinutesNote"] = "Minutter før den låser igen.",
        ["grace"] = "Frist før hårdt luk",
        ["graceNote"] = "Outlook bliver først bedt om at lukke pænt, så den kan nå at spørge om kladder.",
        ["autostart"] = "Start med Windows",
        ["autostartNote"] = "Uden det er låsen væk efter en genstart.",
        ["secMsg"] = "BESKED",
        ["msgNote"] = "Første linje er overskriften. {day}, {time} og {owner} bliver sat ind.",
        ["save"] = "Gem",
        ["savedOk"] = "Gemt",
        ["addApp"] = "Tilføj…",
        ["stateLocked"] = "Låst lige nu",
        ["stateOpen"] = "Åben lige nu",
        ["opensAt"] = "Åbner {0}",
        ["locksAt"] = "Låser kl. {0}",
        ["unlockedUntil"] = "Låst op til {0}",
        ["traySettings"] = "Indstillinger…",
        ["trayQuit"] = "Afslut",
        ["quitPrompt"] = "Afslut fjerner låsen. Skriv din kode:",
        ["defaultMessage"] = "Det er {day}, {owner}.\nHvorfor vil du åbne den? Du skal ikke åbne den.",

        ["problem_badOpenFrom"] = "Åben fra: '{0}' er ikke et klokkeslæt. Brug TT:MM, for eksempel 07:00.",
        ["problem_badOpenUntil"] = "Åben til: '{0}' er ikke et klokkeslæt. Brug TT:MM, for eksempel 17:00.",
        ["problem_backwardsWindow"] = "Åbningstiden {0} går baglæns. Åbningstid krydser ikke midnat.",
        ["problem_emptySosCode"] = "SOS-koden er tom. Så ville SOS låse op på et tomt svar.",
        ["problem_unknownDay"] = "'{0}' er ikke et engelsk dagsnavn (Monday … Sunday).",
        ["problem_noApps"] = "Listen over programmer er tom. Så bliver intet nogensinde låst.",
        ["problem_badSosMinutes"] = "SOS skal låse op i mindst ét minut.",
        ["problem_negativeGrace"] = "Fristen kan ikke være negativ.",
    };
}

