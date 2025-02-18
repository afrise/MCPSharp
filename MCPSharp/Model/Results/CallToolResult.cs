using MCPSharp.Model.Content;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    /// <summary>
    /// Represents the result of a call tool operation.
    /// </summary>
    public class CallToolResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the result is an error.
        /// </summary>
        [JsonPropertyName("isError")]
        public bool IsError { get; set; }

        /// <summary>
        /// Gets or sets the content of the result.
        /// </summary>
        [JsonPropertyName("content")]
        public TextContent[] Content { get; set; }
    }
}
