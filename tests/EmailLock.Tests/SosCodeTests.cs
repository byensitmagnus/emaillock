using Xunit;

namespace EmailLock.Tests;

public class SosCodeTests
{
    static Config WithCode(string code) => new() { SosCode = code };

    [Fact]
    public void Accepts_the_exact_code() =>
        Assert.True(WithCode("TEST-1234").AcceptsSosCode("TEST-1234"));

    [Fact]
    public void Accepts_the_code_with_stray_whitespace_around_it() =>
        Assert.True(WithCode("TEST-1234").AcceptsSosCode("  TEST-1234 "));

    [Fact]
    public void Rejects_a_different_code() =>
        Assert.False(WithCode("TEST-1234").AcceptsSosCode("nope"));

    [Fact]
    public void Is_case_sensitive() =>
        Assert.False(WithCode("TEST-1234").AcceptsSosCode("test-1234"));

    [Fact]
    public void Rejects_an_empty_answer() =>
        Assert.False(WithCode("TEST-1234").AcceptsSosCode(""));

    [Fact]
    public void Rejects_a_missing_answer() =>
        Assert.False(WithCode("TEST-1234").AcceptsSosCode(null));

    // The dangerous case: a blank configured code must not turn every answer -- including
    // an empty one from a cancelled prompt -- into an unlock.
    [Fact]
    public void A_blank_configured_code_accepts_nothing()
    {
        Assert.False(WithCode("").AcceptsSosCode(""));
        Assert.False(WithCode("   ").AcceptsSosCode("   "));
        Assert.False(WithCode("").AcceptsSosCode("anything"));
    }
}
