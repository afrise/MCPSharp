#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using MCPSharp.Model.Schemas;
using System.Text.Json.Serialization;

namespace MCPSharp.Model
{

    public class Tool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("inputSchema")]
        public InputSchema InputSchema { get; set; }
    }

}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
