using MCPSharp.Model;
using MCPSharp.Model.Parameters;
using MCPSharp.Model.Results;
using StreamJsonRpc;
using System.Diagnostics;
using System.IO.Pipelines;

namespace MCPSharp
{
    /// <summary>
    /// MCPSharp Model Context Protocl Client
    /// </summary>
    public class MCPClient : IDisposable
    {
        private readonly string _name;
        private readonly string _version;
        private readonly Process _process;
        private readonly JsonRpc _rpc;
        private List<Tool> _tools;

        /// <summary>
        /// Constructor for the MCP client.
        /// </summary>
        /// <param name="server">the path to the executable</param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="args"></param>
        public MCPClient(string server, string name, string version, params string[] args)
        {
            _name = name;
            _version = version;
            _process = new() { 
                StartInfo = new() { 
                    FileName = server, 
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false, 
                    RedirectStandardInput = true, 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true, 
                    CreateNoWindow = true } };            
            _process.Start();
            var pipe = new DuplexPipe(PipeReader.Create(_process.StandardOutput.BaseStream), PipeWriter.Create(_process.StandardInput.BaseStream));
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()), this);
            _rpc.StartListening();
            _ = _rpc.InvokeAsync<InitializeResult>("initialize", ["2024-11-05", new { roots = new { listChanged = false }, sampling = new { } }, new { name = _name, version = _version }]);
            _ = _rpc.NotifyAsync("notifications/initialized"); 
            _ = GetToolsAsync();
        }

        /// <summary>Gets a list of Tools from the MCP Server</summary>
        /// <returns></returns>
        public async Task<List<Tool>> GetToolsAsync() {
            _tools = (await _rpc.InvokeAsync<ToolsListResult>("tools/list")).Tools;
            return _tools;
        }

        /// <summary>Call a tool with the given name and parameters</summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object> parameters) => 
            await _rpc.InvokeWithParameterObjectAsync<CallToolResult>("tools/call", new ToolCallParameters{ Arguments = parameters, Name = name, Meta=new MetaData() });

        /// <summary>
        /// Call a tool with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<CallToolResult> CallToolAsync(string name) => 
            await _rpc.InvokeWithParameterObjectAsync<CallToolResult>("tools/call", new ToolCallParameters{ Name = name, Arguments = [], Meta = new() });

        /// <summary>
        /// Dispose of the client
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
