if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run <github-pull-request-url>");
    return 1;
}

string url = args[0];
string[] parts = url.Split('/');

if (parts.Length < 7 || !url.Contains("github.com") || parts[5] != "pull")
{
    Console.Error.WriteLine($"Not a valid GitHub pull request URL: {url}");
    Console.Error.WriteLine("Expected format: https://github.com/owner/repo/pull/123");
    return 1;
}

string owner = parts[3];
string repo = parts[4];
string number = parts[6];

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
{
    Console.Error.WriteLine("GITHUB_TOKEN is not set. See the README for setup instructions.");
    return 1;
}

string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/pulls/{number}";
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
    Console.WriteLine(review);
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