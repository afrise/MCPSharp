//namespace MCPSharp.Test
//{
//    [TestClass()]
//    public sealed class SSETransportTests
//    {
//        private static readonly MCPClient? _client;
//        private static readonly MCPClient? client;

//        [ClassInitialize]
//        [Timeout(5000)]
//        public static void ClassInitialize(TestContext context) {
//            Console.WriteLine(context.TestName);
//            // _client = new("Test Client", "1.0.0", "MCPSharp.Example.exe"); //start the exe
//            // client = new(new Uri("http://localhost:8000/sse"), "test_sse_client", "1"); //connect to the sse server 
//        }


//        // [TestMethod("tools/list")] //unucomment this when SSE works
//        [Timeout(10000)]
//        public static async Task TestListToolsAsync() 
//        {
//            Assert.IsTrue(client!.Initialized);
//            var tools = await client.GetToolsAsync();
//            Assert.IsNotNull(tools);
//            Assert.IsTrue(tools.Count > 0);
//            tools.ForEach(tool =>
//            {
//                Assert.IsFalse(string.IsNullOrEmpty(tool.Name));
//                Assert.IsFalse(string.IsNullOrEmpty(tool.Description));
//                Console.WriteLine(tool.Name);
//            });
//        }

//        public class TestTool
//        {
//            [McpTool("test", "test function")] 
//            public static async Task<string> TestFunctionAsync() => await Task.FromResult("test");
//        }

       
//    }
//}
