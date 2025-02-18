using MCPSharp.Model.Capabilities;
using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    /// <summary>
    /// Represents the result of an initialization process.
    /// </summary>
    public class InitializeResult(string protocolVersion, ServerCapabilities capabilities, Implementation serverInfo)
    {
        /// <summary>
        /// Gets or sets the protocol version.
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = protocolVersion;

        /// <summary>
        /// Gets or sets the server capabilities.
        /// </summary>
        [JsonPropertyName("capabilities")]
        public ServerCapabilities Capabilities { get; set; } = capabilities;

        /// <summary>
        /// Gets or sets the server information.
        /// </summary>
        [JsonPropertyName("serverInfo")]
        public Implementation ServerInfo { get; set; } = serverInfo;
    }
}
