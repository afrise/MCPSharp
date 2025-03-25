using MCPSharp.Core.Tools;
using MCPSharp.Core.Transport;
using MCPSharp.Model;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Reflection;

namespace MCPSharp
{
    /// <summary>
    /// Main class for the MCP server.
    /// </summary>
    public class MCPServer
    {
        private static MCPServer _instance = new();

        /// <summary>
        /// Gets the current server instance.
        /// </summary>
        public static MCPServer Instance => _instance;
        private readonly JsonRpc _rpc;
        private readonly Stream StandardOutput;

        private readonly ToolManager _toolManager = new()
        {
            ToolChangeNotification = () =>
            {
                if (EnableToolChangeNotification)
                    _ = _instance._rpc.InvokeWithParameterObjectAsync("notifications/tools/list_changed", null);
            }
        };

        private readonly ResourceManager _resouceManager = new();

        private readonly ServerRpcTarget _target;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// true if tool change notifications are enabled. This will set to true if the client supports it during initialization, but can be disabled.
        /// </summary>
        public static bool EnableToolChangeNotification { get; set; } = false;

        /// <summary>
        /// Enables periodic pings to the client. If the client does not respond, the server will exit. Default is true, as clients are supposed to respond to pings.
        /// </summary>
        public static bool EnablePing { get; set; } = true;

        /// <summary>
        /// The name and version of the server implementation
        /// </summary>
        public Implementation Implementation;

        /// <summary>
        /// Any calls to Console.Write() will be sent to this text writer. Overwrite it if you wish to capture this stream.
        /// </summary>
        public readonly TextWriter RedirectedOutput = TextWriter.Null;

        /// <summary>
        /// Constructor for the MCP server.
        /// </summary>
        private MCPServer()
        {
            Implementation = new();
            _target = new(_toolManager, _resouceManager, Implementation);
            Console.SetOut(RedirectedOutput);
            _rpc = new JsonRpc(new NewLineDelimitedMessageHandler(new StdioTransportPipe(),
                new SystemTextJsonFormatter()
                {
                    JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    }
                }), _target);

            _rpc.StartListening();
        }

        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Call Register<T> instead. The method has been renamed to clear any confusion. They are functionally identical")]
        public static void RegisterTool<T>() where T : class, new() => _instance._toolManager.Register<T>();

        /// <summary>
        /// Registers a tool with the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>() where T : class, new() => _ = _instance.RegisterAsync<T>();
        public async Task RegisterAsync<T>() where T : class, new() { _toolManager.Register<T>(); _resouceManager.Register<T>(); }
        public static void AddToolHandler(Tool tool, Delegate func) => _instance._toolManager.AddToolHandler(new ToolHandler(tool, func.Method));

        /// <summary>
        /// forward Console.WriteLine() to a TextWriter
        /// </summary>
        /// <param name="output"></param>
        public static void SetOutput(TextWriter output) => Console.SetOut(output);

        /// <summary>
        /// Starts the MCP Server, registers all tools, and starts listening for requests.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <param name="version">The version of the server.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task StartAsync(string serverName, string version)
        {
            _instance.Implementation = new(serverName, version);

            var allTypes = Assembly.GetEntryAssembly()!.GetTypes()
                .Where(t =>
                {
                    bool classHasToolAttribute = t.GetCustomAttribute<McpToolAttribute>() != null;
                    bool methodHasToolAttribute = t.GetMethods().Any(m => m.GetCustomAttribute<McpToolAttribute>() != null);
                    bool methodHasFunctionAttribute = t.GetMethods().Any(m => m.GetCustomAttribute<McpFunctionAttribute>() != null);
                    bool methodHasResourceAttribute = t.GetMethods().Any(m => m.GetCustomAttribute<McpResourceAttribute>() != null);

                    return classHasToolAttribute || methodHasToolAttribute || methodHasFunctionAttribute || methodHasResourceAttribute;
                });

            foreach (var toolType in allTypes)
            {
                var registerMethod = typeof(MCPServer).GetMethod(nameof(ToolManager.Register))?.MakeGenericMethod(toolType);
                registerMethod?.Invoke(_instance, null);
            }

            await Task.Delay(-1);
        }

        private async Task StartPingThreadAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(5000);
                if (!EnablePing) break;

                try
                {
                    var response = await _rpc.InvokeWithParameterObjectAsync<object>("ping", null).WithTimeout(TimeSpan.FromMilliseconds(500));

                    if (response == null)
                    {
                        _rpc.Dispose();
                        Environment.Exit(1);
                    }
                }

                catch (Exception)
                {
                    _rpc.Dispose();
                    Environment.Exit(1);
                }
            }
        }

        /// <summary>
        /// Disposes the server and releases all resources.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _rpc.Dispose();
        }
    }
}
