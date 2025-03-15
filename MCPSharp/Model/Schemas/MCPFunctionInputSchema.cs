using MCPSharp.Model.Schemas;
using System.Text.Json.Serialization;

namespace MCPSharp
{
    /// <summary>
    /// This is a wrapper class for an InputSchema - it includes a couple extra fields that make it work with AIFunctions
    /// </summary>
    public class MCPFunctionInputSchema : InputSchema
    {
        /// <summary>
        /// Creates a new MCPFunctionInputSchema from an existing InputSchema
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="schema"></param>
        public MCPFunctionInputSchema(string name, string description, InputSchema schema)
        {
            Name = name;
            Description = description;
            Schema = schema.Schema;
            Required = schema.Required;
            Properties = schema.Properties;
            Type = schema.Type;
            AdditionalProperties = schema.AdditionalProperties;
        }

        /// <summary>
        /// The name of the tool
        /// </summary>
        [JsonPropertyName("title")]
        public string Name;

        /// <summary>
        /// The description of the tool
        /// </summary>
        [JsonPropertyName("description")]
        public string Description; 
    }
}