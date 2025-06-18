using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

#nullable enable

namespace MCPSharp
{
    /// <summary>
    /// Provides a background thread execution model for MCPServer.
    /// This class allows running an MCP server in a background thread with proper lifecycle management.
    /// </summary>
    /// <remarks>
    /// The MCPServerHost class implements IAsyncDisposable for proper cleanup of resources.
    /// It manages server startup, shutdown, and cancellation with timeouts to prevent deadlocks.
    /// 
    /// Example usage:
    /// <code>
    /// await using var server = await MCPServerHost.StartAsync("MyServer", "1.0.0");
    /// // Server is now running in background
    /// // When the using block exits, server will be properly disposed
    /// </code>
    /// </remarks>
    public class MCPServerHost : System.IAsyncDisposable
    {
        private static readonly JoinableTaskFactory JoinableFactory = new(new JoinableTaskContext());

        private static async Task TimeoutAfterAsync(Func<Task> operation, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var timeoutTask = Task.Delay(timeout, cts.Token);
            var operationTask = JoinableFactory.RunAsync(operation);

            var completedTask = await Task.WhenAny(operationTask.Task, timeoutTask).ConfigureAwait(false);
            if (completedTask == timeoutTask)
            {
                throw new TimeoutException();
            }

            await cts.CancelAsync(); // Cancel the timeout task
            await operationTask.Task.ConfigureAwait(false); // Propagate any exceptions
        }

        private Task? _serverLoop;
        private CancellationTokenSource? _cts;
        private readonly AutoResetEvent _ready;
        private bool _disposed;

        private MCPServerHost()
        {
            _ready = new AutoResetEvent(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MCPServerHost));
            }
        }

        /// <summary>
        /// Creates and starts a new MCPServer instance running in a background thread.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="version">The version of the server.</param>
        /// <returns>A running MCPServerHost instance that will manage the server's lifecycle.</returns>
        /// <exception cref="ArgumentException">Thrown when name or version is null or empty.</exception>
        /// <remarks>
        /// The server is started in a background thread and will continue running until disposed.
        /// The method waits for the server to be ready before returning, with a timeout to prevent deadlocks.
        /// </remarks>
        public static async Task<MCPServerHost> StartAsync(string name, string version)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Server name cannot be null or empty.", nameof(name));
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Server version cannot be null or empty.", nameof(version));

            var host = new MCPServerHost();
            await host.InitializeAsync(name, version);
            return host;
        }

        /// <summary>
        /// Initializes the server host and starts the server in a background thread.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="version">The version of the server.</param>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the host has been disposed.</exception>
        /// <exception cref="TimeoutException">Thrown when the server fails to start within the timeout period.</exception>
        /// <remarks>
        /// This method:
        /// 1. Creates a cancellation token source for managing the server lifecycle
        /// 2. Starts the server in a background thread
        /// 3. Waits for the server to signal readiness with a timeout
        /// </remarks>
        private async Task InitializeAsync(string name, string version)
        {
            ThrowIfDisposed();
            const int timeoutSeconds = 5;
            _cts = new CancellationTokenSource();

            // Start server and wait for ready signal
            try
            {
                var serverTask = JoinableFactory.RunAsync(async () =>
                {
                    try
                    {
                        _ready.Set(); // Signal ready before starting server
                        await MCPServer.StartAsync(name, version).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                });
                _serverLoop = serverTask.Task;

                // Wait for ready signal with timeout
                await TimeoutAfterAsync(
                    async () => await JoinableFactory.RunAsync(() => Task.FromResult(_ready.WaitOne())),
                    TimeSpan.FromSeconds(timeoutSeconds)
                ).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await DisposeAsync(); // Clean up on timeout
                throw new TimeoutException("Server failed to start within the timeout period.");
            }
        }

        /// <summary>
        /// Stops the server and releases all resources.
        /// </summary>
        /// <remarks>
        /// This method handles the graceful shutdown of the server:
        /// 1. Cancels the server loop
        /// 2. Waits for completion with a timeout to prevent deadlocks
        /// 3. Cleans up resources including the cancellation token source
        /// 
        /// If the server does not shut down within the timeout period (5 seconds),
        /// the method will still proceed with cleanup to prevent hanging.
        /// </remarks>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_cts != null && _serverLoop != null)
            {
                try
                {
                    // Cancel server and wait for completion with timeout
                    try
                    {
                        await JoinableFactory.RunAsync(async () =>
                        {
                            await Task.Run(() => _cts?.Cancel()).ConfigureAwait(false);
                            if (_serverLoop != null)
                            {
                                await TimeoutAfterAsync(
                                    () => _serverLoop,
                                    TimeSpan.FromSeconds(5)
                                ).ConfigureAwait(false);
                            }
                        }).Task.ConfigureAwait(false);
                    }
                    catch (TimeoutException)
                    {
                        System.Diagnostics.Debug.WriteLine("Server shutdown timed out");
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Server loop terminated with error: {ex}");
                    }
                    finally
                    {
                        MCPServer.Instance.Dispose();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation occurs
                }
                finally
                {
                    _cts.Dispose();
                    _ready.Dispose();
                }
            }
        }
    }
}
