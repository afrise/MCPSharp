using MCPSharp.Core.Tools;
using MCPSharp.Model;
using MCPSharp.Model.Capabilities;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using StreamJsonRpc;

namespace MCPSharp
{
    class ServerRpcTarget(ToolManager toolManager, ResourceManager resourceManager, Implementation implementation)
    {
        private Implementation _clientInfo;
        private ClientCapabilities _clientCapabilities;

        /// <summary>
        /// Initializes the server with the specified protocol version, client capabilities, and client information.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="capabilities">The client capabilities.</param>
        /// <param name="clientInfo">The client information.</param>
        /// <returns>The result of the initialization process.</returns>
        [JsonRpcMethod("initialize")]
        public async Task<InitializeResult> InitializeAsync(string protocolVersion, object capabilities, Implementation clientInfo)
        {
             
            if (capabilities is ClientCapabilities cc)
            {
                _clientCapabilities = cc;
            }
            
            _clientInfo = clientInfo ?? new();
           

            if (_clientCapabilities.Tools.TryGetValue("listChanged", out object value))
            {
                if (value is bool boolValue)
                    MCPServer.EnableToolChangeNotification = boolValue;    
            }

            return await Task.FromResult<InitializeResult>(
                new(protocolVersion, new ServerCapabilities { 
                    Tools = new() { { "listChanged", true } } 
                }, implementation)); 
        }

        /// <summary>
        /// Handles the "notifications/initialized" JSON-RPC method.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JsonRpcMethod("notifications/initialized")]
        public static async Task InitializedAsync() => await Task.Run(() => { });

        /// <summary>
        /// Lists the resources available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of resources.</returns>
        [JsonRpcMethod("resources/list")]
        public async Task<ResourcesListResult> ListResourcesAsync() => await Task.Run(() => new ResourcesListResult() { Resources = resourceManager.Resources });

        /// <summary>
        /// Lists the resource templates available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of resource templates.</returns>
        [JsonRpcMethod("resources/templates/list")]
        public static async Task<ResourceTemplateListResult> ListResourceTemplatesAsync() => await Task.Run(() => new ResourceTemplateListResult());

        /// <summary>
        /// Calls a tool with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the tool call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        [JsonRpcMethod("tools/call", UseSingleObjectParameterDeserialization = true)]
        public async Task<CallToolResult> CallToolAsync(ToolCallParameters parameters) =>
            !toolManager.Tools.TryGetValue(parameters.Name, out var toolHandler)
                ? new CallToolResult { IsError = true, Content = [new Model.Content.TextContent { Text = $"Tool {parameters.Name} not found" }] }
                : await toolHandler.HandleAsync(parameters.Arguments);

        /// <summary>
        /// Lists the tools available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of tools.</returns>
        [JsonRpcMethod("tools/list")]
        public async Task<ToolsListResult> ListToolsAsync(object parameters = null)
        {
            _ = parameters;
            return await Task.FromResult(new ToolsListResult([.. toolManager.Tools.Values.Select(t => t.Tool)]));
        }

        /// <summary>
        /// Pings the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ping response.</returns>
        [JsonRpcMethod("ping")]
        public static async Task<object> PingAsync() => await Task.FromResult<object>(new());

        /// <summary>
        /// Lists the prompts available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of prompts.</returns>
        [JsonRpcMethod("prompts/list")]
        public static async Task<PromptListResult> ListPromptsAsync() => await Task.Run(() => new PromptListResult());
    }
}
