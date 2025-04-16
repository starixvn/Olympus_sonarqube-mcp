namespace CursorMCP.Services;

using Models;

public interface ISonarQubeService
{
    Task<List<SimplifiedSonarQubeIssue>> GetProjectIssuesAsync(string projectKey, int pageNumber = 1, int pageSize = 20);
    Task<IssueSnippetSummaryResponse> GetIssueDetailsAsync(string issueKey);
    Task<RuleFixSuggestionResponse> GetIssueFixSuggestionAsync(string ruleKey);
}