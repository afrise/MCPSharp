using System.IO.Pipelines;
using System.Net.Http;
using System.Net;
using MCPSharp.Core.Transport.SSE;

namespace MCPSharp.Core.Transport
{
    internal class DuplexPipe(Stream reader, Stream writer) : IDuplexPipe
    {
        private readonly PipeReader _reader = PipeReader.Create(reader);
        private readonly PipeWriter _writer = PipeWriter.Create(writer);

        public PipeReader Input => _reader;
        public PipeWriter Output => _writer;
    }

    internal class StdioTransportPipe : IDuplexPipe
    {
        private readonly PipeReader _reader = PipeReader.Create(Console.OpenStandardInput());
        private readonly PipeWriter _writer = PipeWriter.Create(Console.OpenStandardOutput());

        public PipeReader Input => _reader;
        public PipeWriter Output => _writer;
    }

    //internal class SSETransportPipe : IDuplexPipe
    //{
    //    private readonly HttpClient _httpClient = new();
    //    private readonly Uri _address;
    //    public SSETransportPipe(Uri address)
    //    {
    //        _address = address;
    //        _reader = PipeReader.Create( _httpClient.GetStreamAsync(_address).Result);
    //        _writer = PipeWriter.Create(new HttpPostStream(_address.ToString()));
    //    }

    //    private PipeReader _reader;
    //    private PipeWriter _writer;
    //    public PipeReader Input => _reader;
    //    public PipeWriter Output => _writer;
    //}
}