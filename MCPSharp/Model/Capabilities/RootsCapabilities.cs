#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace MCPSharp.Model.Capabilities
{
    public class RootsCapabilities
    {
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
