using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CursorMCP.Api.Models
{
    public class SonarQubeIssue
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("rule")]
        public required string Rule { get; set; }

        [JsonPropertyName("severity")]
        public required string Severity { get; set; }

        [JsonPropertyName("component")]
        public required string Component { get; set; }

        [JsonPropertyName("project")]
        public required string Project { get; set; }

        [JsonPropertyName("line")]
        public int? Line { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;

        [JsonPropertyName("textRange")]
        public TextRange TextRange { get; set; } = new();

        [JsonPropertyName("flows")]
        public List<object> Flows { get; set; } = new();

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("effort")]
        public required string Effort { get; set; }

        [JsonPropertyName("debt")]
        public required string Debt { get; set; }

        [JsonPropertyName("author")]
        public required string Author { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("quickFixAvailable")]
        public bool QuickFixAvailable { get; set; }

        [JsonPropertyName("messageFormattings")]
        public List<object> MessageFormattings { get; set; } = new();

        [JsonPropertyName("codeVariants")]
        public List<object> CodeVariants { get; set; } = new();

        [JsonPropertyName("cleanCodeAttribute")]
        public string CleanCodeAttribute { get; set; } = string.Empty;

        [JsonPropertyName("cleanCodeAttributeCategory")]
        public string CleanCodeAttributeCategory { get; set; } = string.Empty;

        [JsonPropertyName("impacts")]
        public List<Impact> Impacts { get; set; } = new();

        [JsonPropertyName("htmlDescription")]
        public string HtmlDescription { get; set; } = string.Empty;

        [JsonIgnore]
        public CodeSnippet CodeSnippet { get; set; } = new();

        [JsonPropertyName("organization")]
        public required string Organization { get; set; }
    }

    public class TextRange
    {
        [JsonPropertyName("startLine")]
        public int StartLine { get; set; }

        [JsonPropertyName("endLine")]
        public int EndLine { get; set; }

        [JsonPropertyName("startOffset")]
        public int StartOffset { get; set; }

        [JsonPropertyName("endOffset")]
        public int EndOffset { get; set; }
    }

    public class Impact
    {
        [JsonPropertyName("softwareQuality")]
        public string SoftwareQuality { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = string.Empty;
    }

    public class CodeSnippet
    {
        [JsonPropertyName("startLine")]
        public int StartLine { get; set; }

        [JsonPropertyName("endLine")]
        public int EndLine { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }
} 