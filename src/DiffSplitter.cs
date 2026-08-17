using System.Text;
using SharpToken;

public static class DiffSplitter
{
    private static readonly GptEncoding Encoding = GptEncoding.GetEncoding("cl100k_base");

    public static int CountTokens(string text) => Encoding.Encode(text).Count;

    public static IReadOnlyList<string> Split(string diff, int maxTokens)
        => Batch(SplitByFile(diff), maxTokens);

    public static IReadOnlyList<string> SplitByFile(string diff)
    {
        if (string.IsNullOrWhiteSpace(diff))
            return Array.Empty<string>();

        var sections = new List<string>();
        var current = new StringBuilder();

        foreach (var line in diff.Split('\n'))
        {
            if (line.StartsWith("diff --git ") && current.Length > 0)
            {
                sections.Add(current.ToString().TrimEnd('\n'));
                current.Clear();
            }

            current.Append(line).Append('\n');
        }

        if (current.Length > 0)
            sections.Add(current.ToString().TrimEnd('\n'));

        return sections;
    }

    public static IReadOnlyList<string> Batch(IReadOnlyList<string> sections, int maxTokens)
    {
        var batches = new List<string>();
        var current = new StringBuilder();
        int currentTokens = 0;

        foreach (var section in sections)
        {
            string piece = section;
            int pieceTokens = CountTokens(piece);

            if (pieceTokens > maxTokens)
            {
                piece = Truncate(piece, maxTokens) + "\n... (file diff truncated)";
                pieceTokens = CountTokens(piece);
            }

            if (currentTokens > 0 && currentTokens + pieceTokens > maxTokens)
            {
                batches.Add(current.ToString().TrimEnd('\n'));
                current.Clear();
                currentTokens = 0;
            }

            current.Append(piece).Append('\n');
            currentTokens += pieceTokens;
        }

        if (current.Length > 0)
            batches.Add(current.ToString().TrimEnd('\n'));

        return batches;
    }

    private static string Truncate(string text, int maxTokens)
    {
        var tokens = Encoding.Encode(text);

        if (tokens.Count <= maxTokens)
            return text;

        return Encoding.Decode(tokens.Take(maxTokens).ToList());
    }
}