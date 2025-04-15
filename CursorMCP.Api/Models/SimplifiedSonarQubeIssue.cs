using System.Text.Json.Serialization;

namespace CursorMCP.Api.Models
{
    public class SimplifiedSonarQubeIssue
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("rule")]
        public string Rule { get; set; } = string.Empty;

        [JsonPropertyName("component")]
        public string Component { get; set; } = string.Empty;

        [JsonPropertyName("project")]
        public string Project { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
} 