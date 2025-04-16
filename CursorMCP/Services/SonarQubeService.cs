namespace CursorMCP.Services;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using Models;

public class SonarQubeService(HttpClient httpClient, ICacheService cacheService, ISimpleLogService logger) : ISonarQubeService
{
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
    };

    public async Task<List<SimplifiedSonarQubeIssue>> GetProjectIssuesAsync(string projectKey, int pageNumber = 1, int pageSize = 20)
    {
        var cacheKey = $"project_issues_{projectKey}_{pageNumber}_{pageSize}";
        return await cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            logger.Log($"Getting project issues for {projectKey}");
            logger.Log($"Requesting: api/issues/search?componentKeys={projectKey}&resolved=false&ps={pageSize}&p={pageNumber}");

            var response = await httpClient.GetFromJsonAsync<SonarQubeIssuesResponse>(
                $"api/issues/search?componentKeys={projectKey}&resolved=false&ps={pageSize}&p={pageNumber}",
                jsonOptions);

            if (response == null)
                throw new InvalidOperationException("Failed to deserialize SonarQube response");

            // Convert to simplified issues with only required properties
            return response.Issues.Select(issue => new SimplifiedSonarQubeIssue
            {
                Key = issue.Key,
                Rule = issue.Rule,
                Component = issue.Component,
                Project = issue.Project,
                Message = issue.Message,
                Type = issue.Type
            }).ToList();
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<IssueSnippetSummaryResponse> GetIssueDetailsAsync(string issueKey)
    {
        var cacheKey = $"issue_details_{issueKey}";
        return await cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            logger.Log($"Getting issue details for {issueKey}");
            logger.Log($"Requesting: api/sources/issue_snippets?issueKey={issueKey}");

            var response = await httpClient.GetFromJsonAsync<Dictionary<string, SonarQubeIssueDetails>>(
                $"api/sources/issue_snippets?issueKey={issueKey}",
                jsonOptions);

            if (response == null || !response.Any())
                throw new InvalidOperationException("Failed to deserialize SonarQube response");

            var firstItem = response.First();
            var issueDetails = firstItem.Value;

            // Convert HTML code to plain text
            var sources = issueDetails.Sources.Select(s => ConvertHtmlToPlainText(s.Code)).ToList();

            return new IssueSnippetSummaryResponse
            {
                FileName = issueDetails.Component.Name,
                Sources = string.Join("\n", sources)
            };
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<RuleFixSuggestionResponse> GetIssueFixSuggestionAsync(string issueKey)
    {
        var cacheKey = $"issue_fix_{issueKey}";
        return await cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            logger.Log($"Getting issue fix suggestion for {issueKey}");
            logger.Log($"Requesting: api/rules/show?key={issueKey}");

            var response = await httpClient.GetFromJsonAsync<RuleResponse>(
                $"api/rules/show?key={issueKey}",
                jsonOptions);

            if (response == null)
                throw new InvalidOperationException("Failed to deserialize SonarQube response");

            var fixSuggestion = new RuleFixSuggestionResponse();

            foreach (var section in response.Rule.DescriptionSections)
            {
                switch (section.Key)
                {
                    case "introduction":
                        fixSuggestion.Introduction = section.Content;
                        break;
                    case "root_cause":
                        fixSuggestion.RootCause = section.Content;
                        break;
                    case "how_to_fix":
                        fixSuggestion.HowToFix = section.Content;
                        break;
                }
            }

            return fixSuggestion;
        }, TimeSpan.FromMinutes(5));
    }

    private static string ConvertHtmlToPlainText(string html)
    {
        // Remove HTML tags
        var text = Regex.Replace(html, "<[^>]*>", "");

        // Decode HTML entities
        text = HttpUtility.HtmlDecode(text);

        return text;
    }
}

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        return DateTime.Parse(dateString ?? throw new InvalidOperationException("Date string cannot be null"));
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

public class SonarQubeIssuesResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("p")]
    public int Page { get; set; }

    [JsonPropertyName("ps")]
    public int PageSize { get; set; }

    [JsonPropertyName("paging")]
    public Paging Paging { get; set; } = new();

    [JsonPropertyName("effortTotal")]
    public int EffortTotal { get; set; }

    [JsonPropertyName("issues")]
    public List<SonarQubeIssue> Issues { get; set; } = [];

    [JsonPropertyName("components")]
    public List<Component> Components { get; set; } = [];

    [JsonPropertyName("facets")]
    public List<object> Facets { get; set; } = [];
}

public class Paging
{
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class Component
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("qualifier")]
    public string Qualifier { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("longName")]
    public string LongName { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}

public class IssueSnippetResponse
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public List<SourceLine> Sources { get; set; } = [];
}

public class IssueSnippetSummaryResponse
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public string Sources { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"FileName: {FileName}\nSources: {Sources}";
    }
}

public class SourceLine
{
    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public class RuleResponse
{
    [JsonPropertyName("rule")]
    public Rule Rule { get; set; } = new();
}

public class Rule
{
    [JsonPropertyName("descriptionSections")]
    public List<DescriptionSection> DescriptionSections { get; set; } = [];
}

public class DescriptionSection
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class RuleFixSuggestionResponse
{
    [JsonPropertyName("introduction")]
    public string Introduction { get; set; } = string.Empty;

    [JsonPropertyName("root_cause")]
    public string RootCause { get; set; } = string.Empty;

    [JsonPropertyName("how_to_fix")]
    public string HowToFix { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Introduction: {Introduction}\nRoot Cause: {RootCause}\nHow to Fix: {HowToFix}";
    }
}