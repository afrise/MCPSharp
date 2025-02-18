#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace MCPSharp.Model.Capabilities
{
    public class ServerCapabilities
    {
        [JsonPropertyName("tools")] public Dictionary<string, bool> Tools { get; set; } = [];
        [JsonPropertyName("resources")] public Dictionary<string, bool> Resources { get; set; } = [];
        [JsonPropertyName("prompts")] public Dictionary<string, bool> Prompts { get; set; } = [];
        [JsonPropertyName("sampling")] public Dictionary<string, bool> Sampling { get; set; } = [];
        [JsonPropertyName("roots")] public Dictionary<string, bool> Roots { get; set; } = [];
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
