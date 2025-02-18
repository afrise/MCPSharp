using System.Text.Json.Serialization;

namespace MCPSharp.Model
{
    /// <summary>
    /// Represents the implementation details of the MCPSharp server.
    /// </summary>
    public class Implementation
    {
        public Implementation()
        {
        }

        public Implementation(string name, string version) : this() 
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Gets or sets the name of the implementation.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "MCPSharp Server";

        /// <summary>
        /// Gets or sets the version of the implementation.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "0.0.1";
    }
}
