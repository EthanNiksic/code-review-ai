if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run <github-pull-request-url>");
    return 1;
}

string url = args[0];

if (!PullRequestUrl.TryParse(url, out var pr) || pr is null)
{
    Console.Error.WriteLine($"Not a valid GitHub pull request URL: {url}");
    Console.Error.WriteLine("Expected format: https://github.com/owner/repo/pull/123");
    return 1;
}

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
{
    Console.Error.WriteLine("GITHUB_TOKEN is not set. See the README for setup instructions.");
    return 1;
}

string apiUrl = pr.ToApiUrl();
using var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "code-review-ai");
client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.diff");
client.DefaultRequestHeaders.Add("Authorization",
    $"Bearer {Environment.GetEnvironmentVariable("GITHUB_TOKEN")}");

try
{
    var response = await client.GetAsync(apiUrl);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.Error.WriteLine($"Pull request not found: {url}");
        Console.Error.WriteLine("Check the URL, or confirm your token has access if the repo is private.");
        return 1;
    }

    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.Error.WriteLine("GitHub rejected the token. Check that GITHUB_TOKEN is valid.");
        return 1;
    }

    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"GitHub returned {(int)response.StatusCode}.");
        return 1;
    }

    string diff = await response.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(diff))
    {
        Console.Error.WriteLine("This pull request has no changes to review.");
        return 0;
    }

    string review = await LlmClient.ReviewAsync(diff);

    bool post = args.Contains("--post");

    if (post)
    {
        await GitHubClient.PostCommentAsync(client, pr, review);
        Console.Error.WriteLine($"Posted review to {url}");
    }
    else
    {
        Console.WriteLine(review);
    }

    return 0;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Request failed: {ex.Message}");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
