using Xunit;

public class DiffSplitterTests
{
    const string TwoFileDiff = """
        diff --git a/one.cs b/one.cs
        index 111..222 100644
        --- a/one.cs
        +++ b/one.cs
        @@ -1 +1 @@
        -old line one
        +new line one
        diff --git a/two.cs b/two.cs
        index 333..444 100644
        --- a/two.cs
        +++ b/two.cs
        @@ -1 +1 @@
        -old line two
        +new line two
        """;

    [Fact]
    public void SplitsOneSectionPerFile()
    {
        var sections = DiffSplitter.SplitByFile(TwoFileDiff);

        Assert.Equal(2, sections.Count);
        Assert.Contains("one.cs", sections[0]);
        Assert.Contains("two.cs", sections[1]);
        Assert.DoesNotContain("two.cs", sections[0]);
    }

    [Fact]
    public void ReturnsEmptyForEmptyDiff()
    {
        Assert.Empty(DiffSplitter.SplitByFile(""));
    }

    [Fact]
    public void PacksSmallSectionsIntoOneBatch()
    {
        var batches = DiffSplitter.Split(TwoFileDiff, 10_000);

        Assert.Single(batches);
    }

    [Fact]
    public void SeparatesSectionsWhenOverBudget()
    {
        var batches = DiffSplitter.Split(TwoFileDiff, 40);

        Assert.Equal(2, batches.Count);
    }

    [Fact]
    public void TruncatesSectionLargerThanBudget()
    {
        string huge = "diff --git a/big.cs b/big.cs\n" + new string('x', 5000);

        var batches = DiffSplitter.Split(huge, 500);

        Assert.Single(batches);
        Assert.Contains("truncated", batches[0]);
        Assert.True(DiffSplitter.CountTokens(batches[0]) < 600);
    }
}