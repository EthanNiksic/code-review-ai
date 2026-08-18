# code-review-ai

CLI tool that fetches GitHub pull request diffs and generates automated review comments using an LLM.

## Install

```
dotnet tool install -g EthanNiksic.CodeReviewAi
```

If your shell reports `command not found` afterward, add .NET's tool directory to your PATH:

```
export PATH="$PATH:$HOME/.dotnet/tools"
```

## Usage

Generate a fine-grained GitHub personal access token and an OpenAI API key, then:

```
export GITHUB_TOKEN="your_github_token"
export OPENAI_API_KEY="your_openai_key"
code-review-ai https://github.com/owner/repo/pull/123
```

The review is printed to standard output. To post it as a comment on the pull request instead, add `--post`:

```
code-review-ai https://github.com/owner/repo/pull/123 --post
```

Reading diffs requires a token with read access to the repository. Posting comments additionally requires `Pull requests: Read and write`.

Reviews are generated with `gpt-4o-mini` by default.

## How it works

Parses a PR URL into a GitHub API endpoint, then requests the diff using the `application/vnd.github.v3.diff` Accept header, which returns raw diff text instead of JSON. The diff is sent to the OpenAI API, which returns review comments.

URLs are validated and parsed before any network call is made, and failures at each stage — invalid URL, missing credentials, missing pull request, insufficient token permissions — exit with a descriptive message rather than a stack trace.

Diffs larger than the model's context budget are split on file boundaries and packed into batches sized to fit, measured with the same tokenizer the model uses. A single file larger than the budget is truncated and marked as such. The resulting reviews are combined into one output.

Note that pull request diffs are sent to OpenAI's API for processing.

## Built with

C# / .NET 10

## Building from source

```
git clone https://github.com/EthanNiksic/code-review-ai
cd code-review-ai
dotnet run --project src/code-review-ai.csproj https://github.com/owner/repo/pull/123
```

## Running tests

```
dotnet test
```

## Limitations

Reviews are posted as a single summary comment rather than inline on specific lines.

## License

MIT