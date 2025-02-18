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
    class DuplexPipe : IDuplexPipe 
    {
        private readonly PipeReader _reader;
        private readonly PipeWriter _writer;
        public DuplexPipe(PipeReader reader, PipeWriter writer) 
        {
            var r = PipeReader.Create(Console.OpenStandardInput());
            _reader = reader;
            _writer = writer;
        }
        public PipeReader Input => _reader;
        public PipeWriter Output => _writer;
    }
    /// <summary>
    /// Main class for the MCP server.
    /// </summary>
    public partial class MCPServer 
    {
        private readonly Dictionary<string, ToolHandler<object>> tools = [];
        private readonly JsonRpc _rpc;
        private Implementation implementation;

        /// <summary>
        /// Constructor for the MCP server.
        /// </summary>
        public MCPServer()
        {
            var pipe = new DuplexPipe(PipeReader.Create(Console.OpenStandardInput()), PipeWriter.Create(Console.OpenStandardOutput())); 
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
        }

        
        /// <summary>
        /// Starts the MCP Server, Registers all tools and starts listening for requests.
        /// </summary>
        /// <returns></returns>
        public static async Task StartAsync(string serverName, string version)
        {
            var server = new MCPServer
            {
                implementation = new Implementation { Name = serverName, Version = version }
            };

            foreach (var toolType in Assembly.GetEntryAssembly()!.GetTypes().Where(t => t.GetCustomAttribute<McpToolAttribute>() != null))
            {
                server.RegisterTool(toolType);
                var registerMethod = typeof(MCPServer).GetMethod(nameof(RegisterTool))?.MakeGenericMethod(toolType);
                registerMethod?.Invoke(server, null);
            }

            server.Start();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Initializes the MCP server. This is called by the client
        /// </summary>
        /// <param name="protocolVersion"></param>
        /// <param name="capabilities"></param>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        [JsonRpcMethod("initialize")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public InitializeResult Initialize(string protocolVersion, ClientCapabilities capabilities, Implementation clientInfo)
        {
            return new InitializeResult { 
                ProtocolVersion = protocolVersion, 
                Capabilities = new ServerCapabilities
                { 
                    Tools = new() {{"listChaged", false}}, 
                    Resources = [], 
                    Prompts = [], 
                    Sampling = [],
                    Roots = []
                },
                ServerInfo = implementation
            };
        }

        [JsonRpcMethod("notifications/initialized")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static void Initialized(){ }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        [JsonRpcMethod("tools/list")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ToolsListResult ListTools()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var toolsList = tools.Values.Select(t => t.GetToolDefinition()).ToList();
            return new ToolsListResult { Tools = toolsList };
        }


        [JsonRpcMethod("tools/call", UseSingleObjectParameterDeserialization = true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public async Task<CallToolResult> CallToolAsync(ToolCallParameters parameters)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {

            if (!tools.TryGetValue(parameters.Name, out var toolHandler))
            {
                //Log.Error($"Tool {parameters.Name} not found");
                return new CallToolResult { IsError = true, Content = [new TextContent { Text = $"Tool {parameters.Name} not found" }] };

            }

            return await toolHandler.HandleAsync(parameters.Arguments);
        }


        private void RegisterTool(Type type) //where T : class, new()
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
                        Type = p.ParameterType switch {
                            Type t when t == typeof(string) => "string",
                            Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                            Type t when t == typeof(bool) => "boolean",
                            Type t when t.IsArray => "array",
                            Type t when t == typeof(DateTime) => "string",
                            _ => "object"},
                        Description = p.GetXmlDocumentation() ?? p.GetCustomAttribute<McpParameterAttribute>()?.Description ?? "description not set",
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
                        Required = parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key).ToList(),
                    }
                };

                var handler = new ToolHandler<object>(tool, method, instance!);
                tools[tool.Name] = handler;
            }
        }

        private void Start()=>_rpc.StartListening();   
    }
}