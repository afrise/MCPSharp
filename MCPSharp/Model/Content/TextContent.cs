#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace MCPSharp.Model.Content
{
    public class TextContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
