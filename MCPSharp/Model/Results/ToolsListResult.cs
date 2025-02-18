using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    /// <summary>
    /// Represents the result of a tools list operation.
    /// </summary>
    /// <param name="tools">the list of tools</param>
    public class ToolsListResult(List<Tool> tools)
    {
        /// <summary>
        /// Gets or sets the list of tools.
        /// </summary>
        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; } = tools;
    }
}
