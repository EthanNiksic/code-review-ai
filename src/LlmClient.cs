using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class LlmClient
{
    private static readonly HttpClient http = new();

    public const int MaxTokensPerRequest = 20_000;

    public static async Task<string> ReviewAsync(string diff)
    {
        var batches = DiffSplitter.Split(diff, MaxTokensPerRequest);

        if (batches.Count == 0)
            return "";

        if (batches.Count == 1)
            return await ReviewBatchAsync(batches[0]);

        var sb = new StringBuilder();

        for (int i = 0; i < batches.Count; i++)
        {
            Console.Error.WriteLine($"Reviewing part {i + 1} of {batches.Count}...");

            string review = await ReviewBatchAsync(batches[i]);

            sb.AppendLine($"## Part {i + 1} of {batches.Count}");
            sb.AppendLine();
            sb.AppendLine(review);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static async Task<string> ReviewBatchAsync(string diff)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        var payload = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a code reviewer. Give concise, specific feedback on the diff." },
                new { role = "user", content = diff }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API error {(int)response.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}