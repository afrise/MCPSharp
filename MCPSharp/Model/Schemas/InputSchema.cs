using System.Text.Json.Serialization;

namespace MCPSharp.Model.Schemas
{
    /// <summary>
    /// Represents the input schema for a tool function.
    /// </summary>
    public class InputSchema
    {
        /// <summary>
        /// Gets or sets the type of the input schema.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the properties of the input schema.
        /// </summary>
        public Dictionary<string, ParameterSchema> Properties { get; set; }

        /// <summary>
        /// Gets or sets the required properties of the input schema.
        /// </summary>
        public List<string> Required { get; set; }
    }
}
