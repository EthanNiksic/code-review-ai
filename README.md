# code-review-ai

CLI tool that fetches GitHub pull request diffs and generates automated review comments using an LLM.

## Status

Work in progress. Currently fetches and prints PR diffs. LLM review generation is in development.

## Usage

Generate a fine-grained GitHub personal access token with read access to public repositories, then:

    export GITHUB_TOKEN="your_token"
    dotnet run https://github.com/owner/repo/pull/123

## How it works

Parses a PR URL into a GitHub API endpoint, then requests the diff using the `application/vnd.github.v3.diff` Accept header, which returns raw diff text instead of JSON.

## Built with

C# / .NET 10

## Roadmap

- [ ] Send diffs to an LLM for review
- [ ] Handle diffs that exceed model context limits
- [ ] Post comments directly to the PR