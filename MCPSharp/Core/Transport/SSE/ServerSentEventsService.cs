//using Microsoft.AspNetCore.Http;

//namespace MCPSharp.Core.Transport.SSE
//{
    
//    public class ServerSentEventsService
//    {
//        private readonly List<(SseClient Client, MCPServer Instance)> _clients = [];
//        private readonly object _lock = new();
//        private readonly Func<Stream, Stream, MCPServer> _instanceFactory;

//        public ServerSentEventsService(Func<Stream, Stream, MCPServer> instanceFactory)
//        {
//            _instanceFactory = instanceFactory;
//        }

//        public async Task AddClientAsync(HttpContext context, Stream input)
//        {
//            var client = new SseClient(context, input);
//            var instance = _instanceFactory(input, context.Response.Body); 

//            lock (_lock)
//            {
//                _clients.Add((client, instance));
//            }
//        }

//        public async Task RemoveClientAsync(SseClient client)
//        {
//            lock (_lock)
//            {
//                var index = _clients.FindIndex(x => x.Client == client);
//                if (index >= 0)
//                {
//                    _clients[index].Instance.Dispose();
//                    _clients.RemoveAt(index);
//                }
//            }
//        }
//    }
//}