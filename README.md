# code-review-ai

CLI tool that fetches GitHub pull request diffs and generates automated review comments using an LLM.

## Status

Fetches PR diffs and generates LLM-written reviews, with input validation and unit-tested URL parsing. Handling of oversized diffs and posting comments to PRs are still in progress.

## Usage

Generate a fine-grained GitHub personal access token with read access to public repositories, and an OpenAI API key. Then:

```
export GITHUB_TOKEN="your_github_token"
export OPENAI_API_KEY="your_openai_key"
dotnet run https://github.com/owner/repo/pull/123
```

## How it works

Parses a PR URL into a GitHub API endpoint, then requests the diff using the `application/vnd.github.v3.diff` Accept header, which returns raw diff text instead of JSON. The diff is sent to the OpenAI API, which returns review comments printed to standard output.

URLs are validated and parsed before any network call is made, and failures at each stage — invalid URL, missing credentials, missing pull request — exit with a descriptive message rather than a stack trace.

Note that pull request diffs are sent to OpenAI's API for processing.

## Built with

C# / .NET 10

## Running tests

```
dotnet test tests/tests.csproj
```

## Limitations

Large pull requests may exceed the model's context window and will currently fail. Reviews are printed to the terminal rather than posted to the pull request.

## Roadmap

- [x] Send diffs to an LLM for review
- [ ] Handle diffs that exceed model context limits
- [ ] Post comments directly to the PR