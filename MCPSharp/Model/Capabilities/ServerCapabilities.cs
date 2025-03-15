
namespace MCPSharp.Model.Capabilities
{
    /// <summary>
    /// A collection of dictionaries that represent the capabilities of the server. This is sent to the client when negotiating capabilities.
    /// </summary>
    public class ServerCapabilities
    {
        /// <summary>
        /// A dictionary of tool capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> Tools { get; set; } = [];

        /// <summary>
        /// A dictionary of resource capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> Resources { get; set; } = [];

        /// <summary>
        /// A dictionary of prompt capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> Prompts { get; set; } = [];

        /// <summary>
        /// A dictionary of sampling capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> Sampling { get; set; } = [];

        /// <summary>
        /// A dictionary of root capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> Roots { get; set; } = [];

        /// <summary>
        /// A dictionary of logging capabilities that the server supports.
        /// </summary>
        public Dictionary<string, bool> logging { get; set; } = [];
    }
}