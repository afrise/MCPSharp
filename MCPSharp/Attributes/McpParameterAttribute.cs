#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Text.Json.Serialization;

namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class McpParameterAttribute(bool required = false, string description = null) : Attribute
    {
        [JsonPropertyName("required")]
        public bool Required { get; set; } = required;

        [JsonPropertyName("description")]
        public string Description { get; set; } = description;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member