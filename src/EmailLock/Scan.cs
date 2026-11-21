using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EmailLock;

/// <summary>
/// Finds the listed apps among the running processes. This runs every second for as long
/// as the lock is on, which is every night and every weekend, so the cost per call matters.
///
/// Asking Windows for processes by name makes it describe every thread on the machine:
/// about 5 ms per listed app on a machine with 800 processes. The list of process ids alone
/// takes 0.04 ms. So the by-name look is kept for the full look, and between full looks only
/// the ids that were not there a second ago are opened to read their name.
/// </summary>
public sealed unsafe class Scan
{
    // An id can be handed to a new process the moment the old one exits. If that happens
    // between two calls the new process inherits the old verdict, so a full look is taken
    // this often, and whenever the calls have not been back to back.
    const long FullLookEveryMs = 30_000, BackToBackMs = 3_000;

    HashSet<uint> _notOurs = new(), _next = new();
    IReadOnlyList<string>? _apps;
    uint[] _ids = new uint[4096];
    long _lastCall, _lastFullLook;

    /// <summary>Ids of the processes in <paramref name="session"/> whose name is in
    /// <paramref name="apps"/>. Names match as Process.ProcessName does: without ".exe",
    /// ignoring case.</summary>
    public List<int> Find(IReadOnlyList<string> apps, int session)
    {
        var found = new List<int>(0);

        var now = Environment.TickCount64;
        var fullLook = !ReferenceEquals(apps, _apps)
            || now - _lastCall > BackToBackMs || now - _lastFullLook > FullLookEveryMs;
        _lastCall = now;

        uint count;
        while (true)
        {
            fixed (uint* ids = _ids)
                if (!EnumProcesses(ids, (uint)_ids.Length * sizeof(uint), out count)) return found;

            count /= sizeof(uint);
            if (count < _ids.Length) break;
            _ids = new uint[_ids.Length * 2];   // a full buffer may be a cut-off list
        }

        _next.Clear();
        for (var i = 0; i < count && !fullLook; i++)
        {
            var id = _ids[i];
            if (_notOurs.Contains(id)) { _next.Add(id); continue; }

            switch (Look(id, apps, session))
            {
                case Verdict.Ours: found.Add((int)id); break;
                case Verdict.NotOurs: _next.Add(id); break;   // ids that have exited are not carried over

                // A process we may not open, such as an Outlook started as another user,
                // still has to be found. Only the full look can name it.
                default: fullLook = true; break;
            }
        }

        if (fullLook)
        {
            found.Clear();
            _next.Clear();
            _apps = apps;
            _lastFullLook = now;

            try
            {
                foreach (var app in apps)
                    foreach (var p in Process.GetProcessesByName(app))
                        using (p)
                            if (p.SessionId == session) found.Add(p.Id);

                for (var i = 0; i < count; i++)
                    if (!found.Contains((int)_ids[i])) _next.Add(_ids[i]);
            }
            catch { _next.Clear(); }   // nothing learned, so nothing remembered: look again next call
        }

        (_notOurs, _next) = (_next, _notOurs);
        return found;
    }

    enum Verdict { Ours, NotOurs, CannotTell }

    static Verdict Look(uint id, IReadOnlyList<string> apps, int session)
    {
        var process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, id);
        if (process == 0) return Verdict.CannotTell;

        var path = stackalloc char[1024];
        uint length = 1024;
        // The native path: it has no drive letter to fail on, and only the file name is used.
        var ok = QueryFullProcessImageNameW(process, PROCESS_NAME_NATIVE, path, ref length);
        CloseHandle(process);
        if (!ok) return Verdict.CannotTell;

        var name = new ReadOnlySpan<char>(path, (int)length);
        name = name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name = name[..^4];

        for (var i = 0; i < apps.Count; i++)
        {
            if (!name.Equals(apps[i], StringComparison.OrdinalIgnoreCase)) continue;

            // Only this Windows session: another user's Outlook is not ours to close.
            if (!ProcessIdToSessionId(id, out var s)) return Verdict.CannotTell;
            return s == session ? Verdict.Ours : Verdict.NotOurs;
        }
        return Verdict.NotOurs;
    }

    const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000, PROCESS_NAME_NATIVE = 1;

    [DllImport("psapi.dll")] static extern bool EnumProcesses(uint* ids, uint bytes, out uint bytesReturned);
    [DllImport("kernel32.dll")] static extern nint OpenProcess(uint access, bool inherit, uint id);
    [DllImport("kernel32.dll")] static extern bool QueryFullProcessImageNameW(nint process, uint flags, char* path, ref uint length);
    [DllImport("kernel32.dll")] static extern bool ProcessIdToSessionId(uint id, out uint session);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(nint handle);
}
