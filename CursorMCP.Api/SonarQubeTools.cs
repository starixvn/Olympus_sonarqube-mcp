using ModelContextProtocol.Server;
using System.ComponentModel;
using CursorMCP.Services;

[McpServerToolType]
public static class SonarQubeTools
{
    public static ISonarQubeService sonarQubeService { get; set; }

    [McpServerTool, Description("Get project issues")]
    public static async Task<string> GetProjectIssues([Description("The SonarQube project key")] string projectKey)
    {
        try
        {
            var result = await sonarQubeService.GetProjectIssuesAsync(projectKey);

            if (result == null || !result.Any())
            {
                return "No issues found for project";
            }

            // Join the issues into a single string
            return string.Join("\n\n", result.Select(issue => issue.ToString() ?? string.Empty));
        }
        catch (Exception ex)
        {
            return $"Error getting project issues: {ex.Message}\nStack trace: {ex.StackTrace}";
        }
    }

    [McpServerTool, Description("Get project issues")]
    public static async Task<string> GetIssueDetails([Description("issue key")] string issueKey)
    {
        try
        {
            var result = await sonarQubeService.GetIssueDetailsAsync(issueKey);

            if (result == null || string.IsNullOrEmpty(result.FileName))
            {
                return "No details found for issue";
            }

            return result.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error getting issue details: {ex.Message}\nStack trace: {ex.StackTrace}";
        }
    }

    [McpServerTool, Description("Get project issues")]
    public static async Task<string> GetIssueFixSuggestion([Description("rule key")] string ruleKey)
    {            
        try
        {
            var result = await sonarQubeService.GetIssueFixSuggestionAsync(ruleKey);

            if (result == null || (string.IsNullOrEmpty(result.RootCause) &&
                                   string.IsNullOrEmpty(result.Introduction) &&
                                   string.IsNullOrEmpty(result.HowToFix)))
            {
                return "No fix suggestion found for rule";
            }

            return result.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error getting fix suggestion: {ex.Message}\nStack trace: {ex.StackTrace}";
        }
    }
}