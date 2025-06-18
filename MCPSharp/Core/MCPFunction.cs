using MCPSharp.Model;
using MCPSharp.Model.Schemas;
using Microsoft.Extensions.AI;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCPSharp
{
    public class MCPFunctionInputSchema : InputSchema
    {
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

        [JsonPropertyName("title")]
        public string Name;

        [JsonPropertyName("description")]
        public string Description; 
    }

    public class MCPFunctionSchema
    {
        [JsonPropertyName("description")]
        string Description { get; set; }

        [JsonPropertyName("type")]
        string Type { get; set; }

        [JsonPropertyName("properties")]
        Dictionary<string, ParameterSchema> Properties { get; set; }

    }
    public class MCPFunction(Tool tool, MCPClient client) : AIFunction()
    {
        private Tool _tool = tool;
        private readonly MCPClient _client = client;
        public override string Description => _tool.Description;
        public override string Name => _tool.Name;
        public override JsonElement JsonSchema => JsonSerializer.SerializeToElement(new MCPFunctionInputSchema(_tool.Name, _tool.Description, _tool.InputSchema));


        protected override async ValueTask<object> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            return await _client.CallToolAsync(_tool.Name, arguments.ToDictionary(p => p.Key, p => p.Value));
        }
    }
}