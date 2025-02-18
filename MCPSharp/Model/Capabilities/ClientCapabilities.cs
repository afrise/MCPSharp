using System.Text.Json.Serialization;

namespace MCPSharp.Model.Capabilities
{
    /// <summary>
    /// Represents the capabilities of a client.
    /// </summary>
    public class ClientCapabilities
    {
        /// <summary>
        /// Gets or sets a value indicating wether roots are supported.
        /// </summary>
        [JsonPropertyName("roots")]
        public RootsCapabilities Roots { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sampling is supported.
        /// </summary>

        [JsonPropertyName("sampling")]
        public object Sampling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tools are supported.
        /// </summary>
        [JsonPropertyName("tools")]
        public bool Tools { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resources are supported.
        /// </summary>
        [JsonPropertyName("resources")]
        public bool Resources { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether prompts are supported.
        /// </summary>
        [JsonPropertyName("prompts")]
        public bool Prompts { get; set; }
    }
}
