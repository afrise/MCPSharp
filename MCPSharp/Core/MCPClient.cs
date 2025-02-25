using MCPSharp.Model;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using StreamJsonRpc;
using System.Diagnostics;
using System.IO.Pipelines;

namespace MCPSharp
{
    /// <summary>
    /// MCPSharp Model Context Protocol Client.
    /// </summary>
    public class MCPClient : IDisposable
    {
        private readonly string _name;
        private readonly string _version;
        private readonly Process _process;
        private readonly JsonRpc _rpc;
        private List<Tool> _tools;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCPClient"/> class.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <param name="version">The version of the client.</param>
        /// <param name="server">The path to the executable server.</param>
        /// <param name="args">Additional arguments for the server.</param>
        public MCPClient(string name, string version, string server, params string[] args)
        {
            _name = name;
            _version = version;
            _process = new()
            {
                StartInfo = new()
                {
                    FileName = server,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            var pipe = new DuplexPipe(PipeReader.Create(_process.StandardOutput.BaseStream), PipeWriter.Create(_process.StandardInput.BaseStream));
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
            _rpc.StartListening();
            _ = _rpc.InvokeAsync<InitializeResult>("initialize", ["2024-11-05", new { roots = new { listChanged = false }, sampling = new { } }, new { name = _name, version = _version }]);
            _ = _rpc.NotifyAsync("notifications/initialized");
            _ = GetToolsAsync();
        }

        /// <summary>
        /// Gets a list of tools from the MCP server.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of tools.</returns>
        public async Task<List<Tool>> GetToolsAsync()
        {
            _tools = (await _rpc.InvokeWithParameterObjectAsync<ToolsListResult>("tools/list")).Tools;
            return _tools;
        }

        /// <summary>
        /// Calls a tool with the given name and parameters.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <param name="parameters">The parameters to pass to the tool.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object> parameters) =>
            await _rpc.InvokeWithParameterObjectAsync<CallToolResult>("tools/call", new ToolCallParameters { Arguments = parameters, Name = name, Meta = new MetaData() });

        /// <summary>
        /// Calls a tool with the given name.
        /// </summary>
        /// <param name="name">The name of the tool to call.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the tool call.</returns>
        public async Task<CallToolResult> CallToolAsync(string name) =>
            await _rpc.InvokeWithParameterObjectAsync<CallToolResult>("tools/call", new ToolCallParameters { Name = name, Arguments = [], Meta = new() });

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
        public async Task<object> PingAsync() => await _rpc.InvokeWithParameterObjectAsync<object>("ping");

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
            _rpc.Dispose();

            _process.Kill();
            _process.WaitForExit();
            _process.Dispose();
        }
    }
}
