using Xunit;

public class PullRequestUrlTests
{
    [Fact]
    public void ParsesValidUrl()
    {
        bool ok = PullRequestUrl.TryParse(
            "https://github.com/octocat/Hello-World/pull/2879", out var pr);

        Assert.True(ok);
        Assert.NotNull(pr);
        Assert.Equal("octocat", pr!.Owner);
        Assert.Equal("Hello-World", pr.Repo);
        Assert.Equal("2879", pr.Number);
    }

    [Fact]
    public void BuildsCorrectApiUrl()
    {
        PullRequestUrl.TryParse(
            "https://github.com/octocat/Hello-World/pull/2879", out var pr);

        Assert.Equal(
            "https://api.github.com/repos/octocat/Hello-World/pulls/2879",
            pr!.ToApiUrl());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("https://google.com")]
    [InlineData("https://github.com/octocat/Hello-World")]
    [InlineData("https://github.com/octocat/Hello-World/issues/5")]
    [InlineData("https://github.com/octocat/Hello-World/pull/abc")]
    [InlineData("https://github.com/octocat/Hello-World/pull/0")]
    [InlineData("https://evil.com/github.com/octocat/Hello-World/pull/1")]
    public void RejectsInvalidUrl(string url)
    {
        Assert.False(PullRequestUrl.TryParse(url, out _));
    }
}