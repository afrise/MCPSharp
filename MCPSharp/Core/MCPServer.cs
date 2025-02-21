#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using MCPSharp.Core;
using MCPSharp.Model;
using MCPSharp.Model.Capabilities;
using MCPSharp.Model.Content;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using MCPSharp.Model.Schemas;
using StreamJsonRpc;
using System.IO.Pipelines;
using System.Reflection;

namespace MCPSharp
{
    internal class DuplexPipe(PipeReader reader, PipeWriter writer) : IDuplexPipe
    {
        private readonly PipeReader _reader = reader;
        private readonly PipeWriter _writer = writer;

        public PipeReader Input => _reader;
        public PipeWriter Output => _writer;
    }
    /// <summary>
    /// Main class for the MCP server.
    /// </summary>
    public class MCPServer
    {
        private static MCPServer _instance;
        private readonly Dictionary<string, ToolHandler<object>> tools = [];
        private readonly JsonRpc _rpc;
        private readonly Implementation implementation;
        private readonly Stream StandardOutput;

        /// <summary>
        /// The output of Console.WriteLine() will be redirected here. Defaults to null, currently no way to change this.
        /// </summary>
        public readonly TextWriter RedirectedOutput = TextWriter.Null;

        /// <summary>
        /// Constructor for the MCP server.
        /// </summary>
        public MCPServer()
        {
            StandardOutput = Console.OpenStandardOutput();
            Console.SetOut(RedirectedOutput);
            var pipe = new DuplexPipe(PipeReader.Create(Console.OpenStandardInput()), PipeWriter.Create(StandardOutput));
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
        }
      
        /// <summary>
        /// Constructor for the MCP server with implementation details.
        /// </summary>
        /// <param name="implementation">The implementation details of the server.</param>
        public MCPServer(Implementation implementation) : this() => this.implementation = implementation;

        /// <summary>
        /// Constructor for the MCP server with output redirection.
        /// </summary>
        /// <param name="outputWriter">A TextWriter object where any Console.Write() calls will go.</param>
        public MCPServer(TextWriter outputWriter) : this() => RedirectedOutput = outputWriter;

        /// <summary>
        /// Starts the MCP Server, registers all tools, and starts listening for requests.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <param name="version">The version of the server.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task StartAsync(string serverName, string version)
        {
            _instance = new MCPServer(new Implementation(serverName, version));


            foreach (var toolType in Assembly.GetEntryAssembly()!.GetTypes().Where(t => t.GetCustomAttribute<McpToolAttribute>() != null))
            {
                _instance.RegisterTool(toolType);
                var registerMethod = typeof(MCPServer).GetMethod(nameof(RegisterTool))?.MakeGenericMethod(toolType);
                registerMethod?.Invoke(_instance, null);
            }

            _instance.Start();

            await Task.Delay(-1);
        }

        /// <summary>
        /// Initializes the server with the specified protocol version, client capabilities, and client information.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="capabilities">The client capabilities.</param>
        /// <param name="clientInfo">The client information.</param>
        /// <returns>The result of the initialization process.</returns>
        [JsonRpcMethod("initialize")]
        public InitializeResult Initialize(string protocolVersion, ClientCapabilities capabilities, Implementation clientInfo) => new(protocolVersion, new ServerCapabilities { Tools = new() { { "listChanged", false } } }, implementation);

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
        public async Task<ResourcesListResult> ListResourcesAsync() => await Task.Run(() => new ResourcesListResult());

        /// <summary>
        /// Lists the resource templates available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of resource templates.</returns>
        [JsonRpcMethod("resources/templates/list")]
        public async Task<ResourceTemplateListResult> ListResourceTemplatesAsync() => await Task.Run(() => new ResourceTemplateListResult());

        /// <summary>
        /// Calls a tool with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the tool call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        [JsonRpcMethod("tools/call", UseSingleObjectParameterDeserialization = true)]
        public async Task<CallToolResult> CallToolAsync(ToolCallParameters parameters) => !tools.TryGetValue(parameters.Name, out var toolHandler) ? new CallToolResult { IsError = true, Content = [new TextContent { Text = $"Tool {parameters.Name} not found" }] } : await toolHandler.HandleAsync(parameters.Arguments);

        /// <summary>
        /// Lists the tools available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of tools.</returns>
        [JsonRpcMethod("tools/list")]
        public async Task<ToolsListResult> ListToolsAsync() => await Task.Run(() => new ToolsListResult([.. tools.Values.Select(t => t.Tool)]));

        /// <summary>
        /// Pings the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ping response.</returns>
        [JsonRpcMethod("ping")]
        public async Task<object> PingAsync() => await Task.Run(() => new { });

        /// <summary>
        /// Lists the prompts available on the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of prompts.</returns>
        [JsonRpcMethod("prompts/list")]
        public async Task<PromptListResult> ListPromptsAsync() => await Task.Run(() => new PromptListResult());

        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        /// <param name="type">The type of the tool to register.</param>

        private void RegisterTool(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var toolAttr = type.GetCustomAttribute<McpToolAttribute>();
            toolAttr ??= new McpToolAttribute
            {
                Name = type.Name,
                Description = type.GetXmlDocumentation()
            };

            foreach (var method in type.GetMethods())
            {
                var methodAttr = method.GetCustomAttribute<McpFunctionAttribute>();
                if (methodAttr == null) continue;

                methodAttr.Name ??= method.Name;
                methodAttr.Description ??= method.GetXmlDocumentation();

                var parameters = method.GetParameters();
                var parameterSchemas = parameters.ToDictionary(
                    p => p.Name!,
                    p => new ParameterSchema
                    {
                        Type = p.ParameterType switch
                        {
                            Type t when t == typeof(string) => "string",
                            Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                            Type t when t == typeof(bool) => "boolean",
                            Type t when t.IsArray => "array",
                            Type t when t == typeof(DateTime) => "string",
                            _ => "object"
                        },
                        Description = p.GetXmlDocumentation() ?? p.GetCustomAttribute<McpParameterAttribute>()?.Description ?? "",
                        Required = p.GetCustomAttribute<McpParameterAttribute>()?.Required ?? false,
                    }
                );

                var tool = new Tool
                {
                    Name = methodAttr.Name,
                    Description = methodAttr.Description ?? "",
                    InputSchema = new InputSchema
                    {
                        Properties = parameterSchemas,
                        Required = [.. parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key)],
                    }
                };

                var handler = new ToolHandler<object>(tool, method, instance!);
                tools[tool.Name] = handler;
            }
        }


        /// <summary>
        /// Starts the JSON-RPC listener.
        /// </summary>
        private void Start() => _rpc.StartListening();
    }
}