using System.Text;

public static class DiffSplitter
{
    public static IReadOnlyList<string> Split(string diff, int maxChars)
        => Batch(SplitByFile(diff), maxChars);

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

    public static IReadOnlyList<string> Batch(IReadOnlyList<string> sections, int maxChars)
    {
        var batches = new List<string>();
        var current = new StringBuilder();

        foreach (var section in sections)
        {
            string piece = section.Length > maxChars
                ? section[..maxChars] + "\n... (file diff truncated)"
                : section;

            if (current.Length > 0 && current.Length + piece.Length > maxChars)
            {
                batches.Add(current.ToString().TrimEnd('\n'));
                current.Clear();
            }

            current.Append(piece).Append('\n');
        }

        if (current.Length > 0)
            batches.Add(current.ToString().TrimEnd('\n'));

        return batches;
    }
}