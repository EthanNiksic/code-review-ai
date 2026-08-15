string url = args[0];
string[] parts = url.Split('/');

string owner = parts[3];
string repo = parts[4];
string number = parts[6];

string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/pulls/{number}";

using var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "code-review-ai");
client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.diff");
client.DefaultRequestHeaders.Add("Authorization",
    $"Bearer {Environment.GetEnvironmentVariable("GITHUB_TOKEN")}");

string diff = await client.GetStringAsync(apiUrl);
Console.WriteLine(diff);