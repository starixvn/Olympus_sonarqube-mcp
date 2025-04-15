using System.Collections.Generic;
using System.Threading.Tasks;
using CursorMCP.Api.Models;
using CursorMCP.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CursorMCP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SonarQubeController : ControllerBase
    {
        private readonly ISonarQubeService _sonarQubeService;
        private readonly ILogger<SonarQubeController> _logger;

        public SonarQubeController(ISonarQubeService sonarQubeService, ILogger<SonarQubeController> logger)
        {
            _sonarQubeService = sonarQubeService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of issues for a SonarQube project
        /// </summary>
        /// <param name="projectKey">The SonarQube project key</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>A paginated list of issues</returns>
        [HttpGet("projects/{projectKey}/issues")]
        [ProducesResponseType(typeof(PaginatedResponse<List<SonarQubeIssue>>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaginatedResponse<List<SonarQubeIssue>>>> GetProjectIssues(
            string projectKey, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Getting issues for project {ProjectKey} with page {PageNumber} and page size {PageSize}", 
                projectKey, pageNumber, pageSize);

            var paginationParams = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _sonarQubeService.GetProjectIssuesAsync(projectKey, paginationParams);
            
            if (result.Data == null || result.Data.Count == 0)
            {
                _logger.LogWarning("No issues found for project {ProjectKey}", projectKey);
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets details of a specific SonarQube issue
        /// </summary>
        /// <param name="issueKey">The SonarQube issue key</param>
        /// <returns>Issue details including code snippets</returns>
        [HttpGet("issues/{issueKey}")]
        [ProducesResponseType(typeof(IssueSnippetSummaryResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IssueSnippetSummaryResponse>> GetIssueDetails(string issueKey)
        {
            _logger.LogInformation("Getting details for issue {IssueKey}", issueKey);

            var result = await _sonarQubeService.GetIssueDetailsAsync(issueKey);
            
            if (string.IsNullOrEmpty(result.FileName))
            {
                _logger.LogWarning("No details found for issue {IssueKey}", issueKey);
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets fix suggestions for a SonarQube issue
        /// </summary>
        /// <param name="ruleKey">The SonarQube rule key</param>
        /// <returns>Fix suggestions including root cause, introduction, and how to fix</returns>
        [HttpGet("issues/{issueKey}/fix-suggestion")]
        [ProducesResponseType(typeof(RuleFixSuggestionResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RuleFixSuggestionResponse>> GetIssueFixSuggestion(string ruleKey)
        {
            _logger.LogInformation("Getting fix suggestion for rule {RuleKey}", ruleKey);

            var result = await _sonarQubeService.GetIssueFixSuggestionAsync(ruleKey);
            
            if (string.IsNullOrEmpty(result.RootCause) && 
                string.IsNullOrEmpty(result.Introduction) && 
                string.IsNullOrEmpty(result.HowToFix))
            {
                _logger.LogWarning("No fix suggestion found for rule {RuleKey}", ruleKey);
                return NotFound();
            }

            return Ok(result);
        }
    }
} 