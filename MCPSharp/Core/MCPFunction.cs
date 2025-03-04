using MCPSharp.Model;
using MCPSharp.Model.Schemas;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCPSharp
{

    /// <summary>
    /// This class represents a function that can be called by any client that implements the Microsoft.Extensions.AI IChatClient interface.
    /// This is generate automatically to expose tools to that ecosystem.
    /// </summary>
    /// <param name="tool"></param>
    /// <param name="client"></param>
    public class MCPFunction(Tool tool, MCPClient client) : AIFunction()
    {
        private readonly Tool _tool = tool;

        private readonly MCPClient _client = client;

        /// <summary>
        /// Gets a description of the tool.
        /// </summary>
        public override string Description => _tool.Description;

        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        public override string Name => _tool.Name;

        /// <summary>
        /// Gets a JSON schema that describes the input to the tool.
        /// </summary>
        public override JsonElement JsonSchema => JsonSerializer.SerializeToElement(new MCPFunctionInputSchema(_tool.Name, _tool.Description, _tool.InputSchema));


        /// <summary>
        /// Invokes the tool with the specified arguments.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<object> InvokeCoreAsync(IEnumerable<KeyValuePair<string, object>> arguments, CancellationToken cancellationToken)
        {
            return await _client.CallToolAsync(_tool.Name, arguments.ToDictionary(p => p.Key, p => p.Value));
        }
    }
}