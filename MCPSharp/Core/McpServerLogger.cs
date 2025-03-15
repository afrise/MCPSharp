using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.Collections.Concurrent;

namespace MCPSharp
{
    /// <summary>
    /// Logger implementation that sends log messages to a JSON-RPC endpoint.
    /// </summary>
    internal class McpServerLogger : ILogger
    {
        private readonly JsonRpc _rpc;
        private static readonly ConcurrentQueue<LogMessage> _messageBuffer = new ConcurrentQueue<LogMessage>();
        private static bool _clientConnected = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="McpServerLogger"/> class.
        /// </summary>
        /// <param name="rpc">The JSON-RPC client to use for sending log messages.</param>
        public McpServerLogger(JsonRpc rpc)
        {
            _rpc = rpc;
            // Set up event handler for JsonRpc connection established
            _rpc.Disconnected += OnClientDisconnected;
        }

        /// <summary>
        /// Handles the client disconnection event
        /// </summary>
        private void OnClientDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            _clientConnected = false;
            // Notify the MCPServer to properly dispose resources and exit
            MCPServer.HandleClientDisconnected();
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

   

        /// <summary>
        /// Checks if the given <paramref name="logLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">Level to be checked.</param>
        /// <returns>True if enabled, false otherwise.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// Sets the client connection state and sends any buffered messages.
        /// </summary>
        /// <param name="connected">Whether the client is connected.</param>
        public static void SetClientConnected(bool connected)
        {
            _clientConnected = connected;
            if (connected)
            {
                FlushMessageBuffer();
            }
        }

        /// <summary>
        /// Sends all buffered messages to the connected client.
        /// </summary>
        private static void FlushMessageBuffer()
        {
            if (!_clientConnected || _messageBuffer.IsEmpty)
                return;

            // Process all messages in the buffer
            while (_messageBuffer.TryDequeue(out var message))
            {
                message.SendMessageAction();
            }
        }

        /// <summary>
        /// Writes a log entry to the JSON-RPC endpoint or buffers it if the client is not connected.
        /// </summary>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a string message of the state and exception.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            try
            {
                // Format the message using the provided formatter
                string message = formatter(state, exception);
                
                // Create error details including exception info if available
                var details = new Dictionary<string, string>
                {
                    ["message"] = message
                };

                if (exception != null)
                {
                    details["exceptionType"] = exception.GetType().FullName;
                    details["exceptionMessage"] = exception.Message;
                    details["stackTrace"] = exception.StackTrace;
                }
                
                if (eventId.Id != 0)
                {
                    details["eventId"] = eventId.Id.ToString();
                    if (!string.IsNullOrEmpty(eventId.Name))
                    {
                        details["eventName"] = eventId.Name;
                    }
                }

                // Create a delegate to send the message
                Action sendAction = () => 
                {
                    try
                    {
                        _ = _rpc.NotifyWithParameterObjectAsync("notifications/message",
                            new
                            {
                                level = logLevel.ToString().ToLowerInvariant(),
                                logger = "console",
                                data = new
                                {
                                    error = message,
                                    details
                                }
                            });
                    }
                    catch
                    {
                        // Silently handle exceptions when sending messages
                    }
                };

                // Either send immediately if connected or buffer for later
                if (_clientConnected)
                {
                    sendAction();
                }
                else
                {
                    _messageBuffer.Enqueue(new LogMessage(sendAction));
                }
            }
            catch
            {
                // Prevent any logging errors from crashing the application
            }
        }

        /// <summary>
        /// Represents a log message that can be buffered.
        /// </summary>
        private class LogMessage
        {
            public Action SendMessageAction { get; }

            public LogMessage(Action sendMessageAction)
            {
                SendMessageAction = sendMessageAction;
            }
        }
    }
}