using MCPSharp.Core.Transport;
using MCPSharp.Model;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using Microsoft.Extensions.AI;
using StreamJsonRpc;
using System.Diagnostics;

namespace MCPSharp
{

    /// <summary>
    /// MCPSharp Model Context Protocol Client.
    /// </summary>
    public class MCPClient : IDisposable
    {
        /// <summary>
        /// Gets or sets the function that determines whether the client has permission to call a tool with the specified parameters.
        /// </summary>
        public Func <Dictionary<string, object>, bool> GetPermission = (parameters) => true;

        private readonly string _name;
        private readonly string _version;
        private readonly Process _process;
        private readonly JsonRpc _rpc;

        /// <summary>
        /// Gets a value indicating whether the client has been initialized.
        /// </summary>
        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// The tools that have been registered with the client.
        /// </summary>
        public List<Tool> Tools { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MCPClient"/> class.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <param name="version">The version of the client.</param>
        /// <param name="server">The path to the executable server.</param>
        /// <param name="env">Dictionary containing enviroment variables</param>
        /// <param name="args">Additional arguments for the server.</param>
        public MCPClient(string name, string version, string server, string args = null, IDictionary<string, string> env =null)
        {

            ProcessStartInfo startInfo = new(server, args)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };


            foreach (var envvar in env?? new Dictionary<string,string>())
            {
                startInfo.EnvironmentVariables.Add(envvar.Key, envvar.Value);
            }

            _name = name;
            _version = version;

            _process = new() { StartInfo = startInfo };
            _process.Start();

            var pipe = new DuplexPipe(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream);
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter() { 
                JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions { 
                    PropertyNameCaseInsensitive = true, 
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
                } }), this);
            _rpc.StartListening();

            _ = _rpc.InvokeAsync<InitializeResult>("initialize", 
                    [
                        "2024-11-05", 
                        new { 
                            roots = new { listChanged = false }, 
                            sampling = new { }, 
                            tools = new { listChanged = true } 
                        },
                            
                        new { 
                            name = _name, 
                            version = _version }
                    ]);

            _ = _rpc.NotifyAsync("notifications/initialized");
            _ = GetToolsAsync();
        }

        /// <summary>
        /// MCP standard ping function
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("ping")]
        public static async Task<object> RecievePingAsync() => await Task.FromResult<object>(new());


        /// <summary>
        /// MCP tools list changed notification
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("notifications/tools/list_changed")]
        public async Task ToolsListChangedAsync() => await GetToolsAsync();



        /// <summary>
        /// Expose tools as Microsoft.Extensions.AI AIFunctions
        /// </summary>
        /// <returns></returns>
        public async Task<IList<AIFunction>> GetFunctionsAsync()  
        {
            List<AIFunction> functions = []; 

            await GetToolsAsync();

            foreach (var tool in Tools)
            {
                MCPFunction function = new(tool, this); 
                functions.Add(function);
            }

            return functions;
        }


        /// <summary>
        /// Gets a list of tools from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of tools.</returns>
        public async Task<List<Tool>> GetToolsAsync()
        {
            Tools = (await _rpc.InvokeWithParameterObjectAsync<ToolsListResult>("tools/list")).Tools;
            return Tools;
        }
        
        /// <summary>
        /// Calls a tool with the given name and parameters.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <param name="parameters">The parameters to pass to the tool.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object> parameters)
        {
            if (!GetPermission(new Dictionary<string, object> { ["tool"] = name, ["parameters"] = parameters }))
                return new CallToolResult() { 
                    IsError = true, 
                    Content = [new Model.Content.TextContent() { 
                        Text = "Permission Denied." }] 
                };
                
            return await _rpc.InvokeWithParameterObjectAsync<CallToolResult>(
                "tools/call", new ToolCallParameters { Arguments = parameters, Name = name });
        }
        

        /// <summary>
        /// Calls a tool with the given name.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name) => await CallToolAsync(name, []);

        /// <summary>
        /// Gets a list of resources from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of resources.</returns>
        public async Task<ResourcesListResult> GetResourcesAsync() => await _rpc.InvokeWithParameterObjectAsync<ResourcesListResult>("resources/list");

        /// <summary>
        /// Gets a list of resource templates from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of resource templates.</returns>
        public async Task<ResourceTemplateListResult> GetResourceTemplatesAsync() => await _rpc.InvokeWithParameterObjectAsync<ResourceTemplateListResult>("resources/templates/list");

        /// <summary>
        /// Pings the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ping response.</returns>
        public async Task<object> SendPingAsync() => await _rpc.InvokeWithParameterObjectAsync<object>("ping");

        /// <summary>
        /// Gets a list of prompts from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of prompts.</returns>
        public async Task<PromptListResult> GetPromptListAsync() => await _rpc.InvokeWithParameterObjectAsync<PromptListResult>("prompts/list");

        /// <summary>
        /// Releases all resources used by the <see cref="MCPClient"/> class.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
            _ = _rpc.DispatchCompletion;

            _rpc.Dispose();

            _process.Kill();
            _process.WaitForExit();
            _process.Dispose();
        }
    }
}
