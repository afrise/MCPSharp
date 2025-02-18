#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace MCPSharp
{
    public class ResourcesListResult
    {
        [JsonPropertyName("resources")]
        public List<Resource> Resources { get; set; } = [];
    }

    public class Resource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }

    public class ResourceTemplateListResult
    {
        [JsonPropertyName("resourceTemplates")]
        public List<Template> Templates { get; set; } = [];

    }

    public class Template
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("uriTemplate")]
        public string UriTemplate { get; set; } 
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
