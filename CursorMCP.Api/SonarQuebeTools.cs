using System.Collections.Generic;
using System.Threading.Tasks;
using CursorMCP.Api.Models;
using CursorMCP.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;

namespace CursorMCP.Api
{
    [McpServerToolType]
    public static class SonarQuebeTools
    {
        [McpServerTool, Description("Get project issues")]
        public static async Task<string> GetProjectIssues(
            ISonarQubeService sonarQubeService,
            [Description("The SonarQube project key")] string projectKey)
        {
            const int pageNumber = 1;
            const int pageSize = 20;

            var result = await sonarQubeService.GetProjectIssuesAsync(projectKey, pageNumber, pageSize);

            if (result == null || !result.Any())
            {
                return "No issues found for project";
            }

            // Join the issues into a single string
            return string.Join("\n\n", result.Select(issue => issue.ToString() ?? string.Empty));
        }

        [McpServerTool, Description("Get project issues")]
        public static async Task<string> GetIssueDetails(
            ISonarQubeService sonarQubeService,
            [Description("issue key")] string issueKey)
        {
            var result = await sonarQubeService.GetIssueDetailsAsync(issueKey);

            if (result == null || string.IsNullOrEmpty(result.FileName))
            {
                return "No details found for issue";
            }

            return result.ToString() ?? string.Empty;
        }

        [McpServerTool, Description("Get project issues")]
        public static async Task<string> GetIssueFixSuggestion(
            ISonarQubeService sonarQubeService,
            [Description("rule key")] string ruleKey)
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
    }
}