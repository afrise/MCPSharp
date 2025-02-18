#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class ToolsListResult
    {
        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member