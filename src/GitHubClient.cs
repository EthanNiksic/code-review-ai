using System.Text;
using System.Text.Json;

public static class GitHubClient
{
    public static async Task PostCommentAsync(HttpClient client, PullRequestUrl pr, string body)
    {
        string url = $"https://api.github.com/repos/{pr.Owner}/{pr.Repo}/issues/{pr.Number}/comments";

        var payload = new { body };

        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
            response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "GitHub rejected the comment. GITHUB_TOKEN needs Pull requests: Read and write on this repository.");
        }

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to post comment ({(int)response.StatusCode}): {error}");
        }
    }
}