using System.Diagnostics;
using Xunit;

namespace EmailLock.Tests;

/// <summary>The one process a test can count on is the one running it.</summary>
public class ScanTests
{
    static readonly Process Me = Process.GetCurrentProcess();
    static readonly string[] MyName = { Me.ProcessName };

    [Fact]
    public void Finds_a_listed_process_in_this_session() =>
        Assert.Contains(Me.Id, new Scan().Find(MyName, Me.SessionId));

    [Fact]
    public void Still_finds_it_on_every_later_call()
    {
        var scan = new Scan();
        scan.Find(MyName, Me.SessionId);

        Assert.Contains(Me.Id, scan.Find(MyName, Me.SessionId));
        Assert.Contains(Me.Id, scan.Find(MyName, Me.SessionId));
    }

    [Fact]
    public void Matches_the_name_whatever_its_case() =>
        Assert.Contains(Me.Id, new Scan().Find(new[] { Me.ProcessName.ToUpperInvariant() }, Me.SessionId));

    [Fact]
    public void Leaves_another_session_alone() =>
        Assert.Empty(new Scan().Find(MyName, Me.SessionId + 1000));

    [Fact]
    public void Finds_nothing_when_nothing_listed_is_running() =>
        Assert.Empty(new Scan().Find(new[] { "no-such-app-7c1e" }, Me.SessionId));

    [Fact]
    public void Finds_nothing_when_nothing_is_listed() =>
        Assert.Empty(new Scan().Find(Array.Empty<string>(), Me.SessionId));

    // Every id is remembered as "not ours" after the first call. Saving a new app list
    // in Settings must not leave that memory standing, or the new app would never lock.
    [Fact]
    public void A_changed_app_list_is_looked_at_afresh()
    {
        var scan = new Scan();
        Assert.Empty(scan.Find(new[] { "no-such-app-7c1e" }, Me.SessionId));

        Assert.Contains(Me.Id, scan.Find(MyName, Me.SessionId));
    }
}
