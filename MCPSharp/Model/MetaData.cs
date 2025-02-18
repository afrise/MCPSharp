using System.Text.Json.Serialization;

namespace MCPSharp.Model
{
    /// <summary>
    /// Represents metadata information.
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// Gets or sets the progress token.
        /// </summary>
        [JsonPropertyName("progressToken")]
        public int ProgressToken { get; set; }
    }
}
