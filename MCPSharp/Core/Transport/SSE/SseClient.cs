using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Threading;
using System.IO.Pipelines;
using System.Text;

namespace MCPSharp.Core.Transport.SSE
{
    /// <summary>
    /// Represents a connection to a client using Server-Sent Events.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="session"></param>
    public class SseServerTransport(string endpoint, ServerSseSession session)
    {
        private readonly string _endpoint = endpoint;
        private readonly ServerSseSession _session = session;
        private bool _initialized = false;

        /// <summary>
        /// The session ID for the connection.
        /// </summary>
        public readonly string SessionId = Guid.NewGuid().ToString();

        /// <summary>
        /// Starts the connection to the client.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task StartAsync()
        {
            if (_initialized)
            {
                throw new InvalidOperationException("SseServerTransport already started! If using Server class, note that connect() calls start() automatically.");
            }

            _initialized = true;

            await _session.SendAsync("endpoint", $"{Uri.EscapeDataString(_endpoint)}?sessionId={SessionId}");

            try
            {
                await _session.CancellationToken.WaitHandle.ToTask();
            }
            finally
            {
                OnClose?.Invoke();
            }
        }

        /// <summary>
        /// Handles a POST request from the client.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task HandlePostMessageAsync(HttpContext context)
        {
            if (!_initialized)
            {
                var message = "SSE connection not established";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(message);
                OnError?.Invoke(new InvalidOperationException(message));
                return;
            }

            string body;
            try
            {
                if (!context.Request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Unsupported content-type: {context.Request.ContentType}");
                }

                using var reader = new StreamReader(context.Request.Body);
                body = await reader.ReadToEndAsync();
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Invalid message: {e.Message}");
                OnError?.Invoke(e);
                return;
            }

            try
            {
                await HandleMessageAsync(body);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Error handling message {body}: {e.Message}");
                return;
            }

            context.Response.StatusCode = 202;
            await context.Response.WriteAsync("Accepted");
        }

        /// <summary>
        /// Handles a message received from the client.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task HandleMessageAsync(string message)
        {
            try
            {
                var parsedMessage = JsonSerializer.Deserialize<JsonRpcMessage>(message);
                await Task.Run(() => OnMessage?.Invoke(parsedMessage));
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                throw;
            }
        }
        /// <summary>
        /// Closes the connection to the client.
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await _session.CloseAsync();
            OnClose?.Invoke();
        }
        /// <summary>
        /// sse
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendAsync(JsonRpcMessage message)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Not connected");
            }

            var jsonMessage = JsonSerializer.Serialize(message);
            await _session.SendAsync("message", jsonMessage);
        }

        /// <summary>
        ///  the action that is called when a message is received from the client.
        /// </summary>
        public event Action<JsonRpcMessage> OnMessage;

        /// <summary>
        /// the action that is called when the connection is closed.
        /// </summary>
        public event Action OnClose;

        /// <summary>
        /// the action that is called when an error occurs.
        /// </summary>
        public event Action<Exception> OnError;
    }

    /// <summary>
    /// Represents a connection to a client using Server-Sent Events.
    /// </summary>
    public class ServerSseSession(IDuplexPipe duplexPipe)
    {
        
        private bool connected = true;
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSseSession"/> class.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SendAsync(string eventName, string data)
        {
            if (!connected) return;

            await duplexPipe.Output.WriteAsync(UTF8Encoding.UTF8.GetBytes($"event: {eventName}\n"));
            await duplexPipe.Output.WriteAsync(UTF8Encoding.UTF8.GetBytes($"data: {data}"));
        }

        /// <summary>
        /// Closes the connection to the client.
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            connected = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// not needed
    /// </summary>
    public class JsonRpcMessage
    {
        /* Implementation */
    }
}