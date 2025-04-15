using System.Collections.Generic;
using System.Threading.Tasks;
using CursorMCP.Api.Models;

namespace CursorMCP.Api.Services
{
    public interface ISonarQubeService
    {
        Task<PaginatedResponse<List<SonarQubeIssue>>> GetProjectIssuesAsync(string projectKey, PaginationParameters paginationParams);
        Task<IssueSnippetSummaryResponse> GetIssueDetailsAsync(string issueKey);
        Task<RuleFixSuggestionResponse> GetIssueFixSuggestionAsync(string ruleKey);
    }
} 