namespace MCPSharp.Test
{
    [TestClass]
    public sealed class ImplementationTests 
    {
        private readonly static MCPClient client = new("MCPSharp.Example.exe", "Test Client", "1.0.0");

        [ClassCleanup] 
        public static void ClassCleanup() { client?.Dispose(); }

        [TestMethod("Tools/List")]
        public async Task Test_ListTools()  
        {
            var tools = await client.GetToolsAsync();
            Assert.IsNotNull(tools);
            Assert.IsTrue(tools.Count > 0);
            tools.ForEach(tool =>
            {
                Assert.IsFalse(string.IsNullOrEmpty(tool.Name));
                Assert.IsFalse(string.IsNullOrEmpty(tool.Description));
            });
        }

        [TestMethod("Tools/Call")]
        public async Task TestCallTool()
        {
            var result = await client.CallToolAsync("Hello");
            string response = result.Content[0].Text;
            Assert.AreEqual("hello, claude.", response);
        }

        [TestMethod("Tools/Call with Parameters")]
        public async Task TestCallToolWithParameters()
        {
            var result = await client.CallToolAsync("Echo", new Dictionary<string, object>{{ "input", "this is a test of the echo function" }});
            string response = result.Content[0].Text;
            Assert.AreEqual("this is a test of the echo function", response);
        }

        [TestMethod("Exception Handling")]
        public async Task TestException()
        {
            var result = await client.CallToolAsync("throw_exception");
            string response = result.Content[0].Text;
            Assert.AreEqual("This is an exception", response);
        }

        [TestMethod("Tools/Call with Invalid Tool")]
        public async Task TestCallInvalidTool()
        {
            Assert.IsTrue((await client.CallToolAsync("NotARealTool")).IsError);
        }

        [TestMethod("Tools/Call with Invalid Parameters")]
        public async Task TestCallToolWithInvalidParameters()
        {
            var result = await client.CallToolAsync("Echo", new Dictionary<string, object> { { "invalid_param", "test" } });
            Assert.IsTrue(result.IsError);
        }
    }
}
