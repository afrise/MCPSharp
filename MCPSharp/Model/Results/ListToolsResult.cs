using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    /// <summary>
    /// Represents the result of listing tools.
    /// </summary>
    public class ListToolsResult
    {
        /// <summary>
        /// Gets or sets the list of tools.
        /// </summary>
        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the tools.
        /// </summary>
        [JsonPropertyName("_meta")]
        public Dictionary<string, object> Meta { get; set; }
    }
}
