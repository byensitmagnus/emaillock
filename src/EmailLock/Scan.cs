using System.Runtime.InteropServices;

namespace EmailLock;

/// <summary>
/// Finds the listed apps among the running processes. This runs every second for as long
/// as the lock is on, which is every night and every weekend, so the cost per call matters.
///
/// Reading every process by name (Process.GetProcessesByName, a toolhelp snapshot, WTS) makes
/// Windows describe every thread on the machine: about 5 ms per listed app on a machine with
/// 800 processes. The list of process ids alone takes 0.04 ms. So only ids not seen on the
/// previous call are opened to read their name; the rest are already known not to be ours.
/// </summary>
public sealed unsafe class Scan
{
    // An id can be handed to a new process the moment the old one exits. If that happens
    // between two calls the new process inherits the old verdict, so verdicts are thrown
    // away this often, and whenever the calls have not been back to back.
    const long ForgetEveryMs = 30_000, BackToBackMs = 3_000;

    HashSet<uint> _notOurs = new(), _next = new();
    IReadOnlyList<string>? _apps;
    uint[] _ids = new uint[4096];
    long _lastCall, _lastForgot;

    /// <summary>Ids of the processes in <paramref name="session"/> whose name is in
    /// <paramref name="apps"/>. Names match as Process.ProcessName does: without ".exe",
    /// ignoring case.</summary>
    public List<int> Find(IReadOnlyList<string> apps, int session)
    {
        var found = new List<int>(0);

        var now = Environment.TickCount64;
        if (!ReferenceEquals(apps, _apps) || now - _lastCall > BackToBackMs || now - _lastForgot > ForgetEveryMs)
        {
            _notOurs.Clear();
            _apps = apps;
            _lastForgot = now;
        }
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
        for (var i = 0; i < count; i++)
        {
            var id = _ids[i];

            // Only this Windows session: another user's Outlook is not ours to close.
            if (!_notOurs.Contains(id) && IsListed(id, apps)
                && ProcessIdToSessionId(id, out var s) && s == session)
                found.Add((int)id);
            else
                _next.Add(id);   // ids that have exited are dropped by not being carried over
        }
        (_notOurs, _next) = (_next, _notOurs);

        return found;
    }

    static bool IsListed(uint id, IReadOnlyList<string> apps)
    {
        // Fails for system processes we may not open. None of those is a mail client.
        var process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, id);
        if (process == 0) return false;

        var path = stackalloc char[1024];
        uint length = 1024;
        var ok = QueryFullProcessImageNameW(process, 0, path, ref length);
        CloseHandle(process);
        if (!ok) return false;

        var name = new ReadOnlySpan<char>(path, (int)length);
        name = name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name = name[..^4];

        for (var i = 0; i < apps.Count; i++)
            if (name.Equals(apps[i], StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    [DllImport("psapi.dll")] static extern bool EnumProcesses(uint* ids, uint bytes, out uint bytesReturned);
    [DllImport("kernel32.dll")] static extern nint OpenProcess(uint access, bool inherit, uint id);
    [DllImport("kernel32.dll")] static extern bool QueryFullProcessImageNameW(nint process, uint flags, char* path, ref uint length);
    [DllImport("kernel32.dll")] static extern bool ProcessIdToSessionId(uint id, out uint session);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(nint handle);
}
