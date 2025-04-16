using System.Collections.Generic;
using System.Threading.Tasks;
using CursorMCP.Api.Models;

namespace CursorMCP.Api.Services
{
    public interface ISonarQubeService
    {
        Task<List<SimplifiedSonarQubeIssue>> GetProjectIssuesAsync(string projectKey, int pageNumber = 1, int pageSize = 20);
        Task<IssueSnippetSummaryResponse> GetIssueDetailsAsync(string issueKey);
        Task<RuleFixSuggestionResponse> GetIssueFixSuggestionAsync(string ruleKey);
    }
} 