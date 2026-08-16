public record PullRequestUrl(string Owner, string Repo, string Number)
{
    public string ToApiUrl() =>
        $"https://api.github.com/repos/{Owner}/{Repo}/pulls/{Number}";

    public static bool TryParse(string url, out PullRequestUrl? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Host != "github.com" && uri.Host != "www.github.com")
            return false;

        string[] parts = uri.AbsolutePath.Trim('/').Split('/');

        if (parts.Length < 4 || parts[2] != "pull")
            return false;

        if (!int.TryParse(parts[3], out int number) || number <= 0)
            return false;

        result = new PullRequestUrl(parts[0], parts[1], number.ToString());
        return true;
    }
}