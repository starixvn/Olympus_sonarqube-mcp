using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CursorMCP.Api.Models
{
    public class SonarQubeIssueDetails
    {
        [JsonPropertyName("component")]
        public Component Component { get; set; } = new();

        [JsonPropertyName("sources")]
        public List<Source> Sources { get; set; } = new();
    }

    public class Component
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("longName")]
        public string LongName { get; set; } = string.Empty;

        [JsonPropertyName("q")]
        public string Q { get; set; } = string.Empty;

        [JsonPropertyName("project")]
        public string Project { get; set; } = string.Empty;

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("measures")]
        public Measures Measures { get; set; } = new();
    }

    public class Measures
    {
        [JsonPropertyName("lines")]
        public string Lines { get; set; } = string.Empty;

        [JsonPropertyName("issues")]
        public string Issues { get; set; } = string.Empty;
    }

    public class Source
    {
        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("scmRevision")]
        public string ScmRevision { get; set; } = string.Empty;

        [JsonPropertyName("scmAuthor")]
        public string ScmAuthor { get; set; } = string.Empty;

        [JsonPropertyName("scmDate")]
        public DateTime ScmDate { get; set; }

        [JsonPropertyName("duplicated")]
        public bool Duplicated { get; set; }

        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }
    }
} 