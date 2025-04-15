using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CursorMCP.Api.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CursorMCP.Api.Services
{
    public class SonarQubeService : ISonarQubeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SonarQubeService> _logger;

        public SonarQubeService(HttpClient httpClient, IConfiguration configuration, ICacheService cacheService, ILogger<SonarQubeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
            };

            var baseUrl = _configuration["SonarQube:BaseUrl"];
            var token = _configuration["SonarQube:Token"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("SonarQube configuration is missing. Please check appsettings.json");
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:")));
        }

        public async Task<PaginatedResponse<List<SonarQubeIssue>>> GetProjectIssuesAsync(string projectKey, PaginationParameters parameters)
        {
            var cacheKey = $"project_issues_{projectKey}_{parameters.PageNumber}_{parameters.PageSize}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<SonarQubeIssuesResponse>(
                    $"api/issues/search?componentKeys={projectKey}&p={parameters.PageNumber}&ps={parameters.PageSize}",
                    _jsonOptions);

                if (response == null)
                    throw new InvalidOperationException("Failed to deserialize SonarQube response");

                return new PaginatedResponse<List<SonarQubeIssue>>(
                    response.Issues,
                    response.Total,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<IssueSnippetSummaryResponse> GetIssueDetailsAsync(string issueKey)
        {
            var cacheKey = $"issue_details_{issueKey}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<IssueSnippetSummaryResponse>(
                    $"api/sources/issue_snippets?issueKey={issueKey}",
                    _jsonOptions);

                if (response == null)
                    throw new InvalidOperationException("Failed to deserialize SonarQube response");

                return response;
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<RuleFixSuggestionResponse> GetIssueFixSuggestionAsync(string issueKey)
        {
            var cacheKey = $"issue_fix_{issueKey}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<RuleFixSuggestionResponse>(
                    $"api/rules/fix_suggestions?issueKey={issueKey}",
                    _jsonOptions);

                if (response == null)
                    throw new InvalidOperationException("Failed to deserialize SonarQube response");

                return response;
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
        public List<SonarQubeIssue> Issues { get; set; } = new();

        [JsonPropertyName("components")]
        public List<Component> Components { get; set; } = new();

        [JsonPropertyName("facets")]
        public List<object> Facets { get; set; } = new();
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
        public List<SourceLine> Sources { get; set; } = new();
    }

    public class IssueSnippetSummaryResponse
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("sources")]
        public string Sources { get; set; } = string.Empty;
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
        public List<DescriptionSection> DescriptionSections { get; set; } = new();
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
    }
}