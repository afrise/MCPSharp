using MCPSharp.Example;

namespace MCPSharp.Test
{
    [TestClass]
    public sealed class STDIOTransportTests
    {
        private readonly static MCPClient client = new("Test Client", "1.0.0", "dotnet", "MCPSharp.Example.dll");

        [ClassCleanup]
        public static void ClassCleanup() { client?.Dispose(); }

        [TestCategory("Tools")]
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

                Console.WriteLine(tool.Name);
            });
        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call")]
        public async Task TestCallTool()
        {
            var result = await client.CallToolAsync("Hello");
            string response = result.Content[0].Text;

            Assert.AreEqual("hello, claude.", response);
            Assert.AreEqual("text", result.Content[0].Type);
        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with a dynamically created tool")]
        public async Task TestCallDynamicTool()
        {
            var result = await client.CallToolAsync("dynamicTool", new Dictionary<string, object> { { "input", "test string" }, { "input2", "another string" } });
            string response = result.Content[0].Text;
            Assert.AreEqual("hello, test string.\nanother string", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with semantic kernel function")]
        public async Task TestCallTool_semantic()
        {
            var result = await client.CallToolAsync("SemanticTest");
            string response = result.Content[0].Text;
            Assert.AreEqual("success", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with Parameters")]
        public async Task TestCallToolWithParameters()
        {
            var result = await client.CallToolAsync("Echo", new Dictionary<string, object> { { "input", "this is a test of the echo function" } });
            Assert.IsFalse(result.IsError);
            string response = result.Content[0].Text;
            Assert.AreEqual("this is a test of the echo function", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }

        [TestCategory("Misc")]
        [TestMethod("Exception Handling")]
        public async Task TestException()
        {
            var result = await client.CallToolAsync("throw_exception");
            string response = result.Content[0].Text;
            Assert.AreEqual("This is an exception", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with Invalid Tool")]
        public async Task TestCallInvalidTool()
        {
            Assert.IsTrue((await client.CallToolAsync("NotARealTool")).IsError);
        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with Invalid Parameters")]
        public async Task TestCallToolWithInvalidParameters()
        {
            var result = await client.CallToolAsync("Echo", new Dictionary<string, object> { { "invalid_param", "test" } });
            Assert.IsTrue(result.IsError);
        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with dll tool")] 
        public async Task TestCallExternalTool()
        {
            var result = await client.CallToolAsync("dll-tool");
            string response = result.Content[0].Text;
            Assert.AreEqual("success", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }


        [TestCategory("Prompts")]
        [TestMethod("Prompts/List")]
        public async Task TestListPrompts()
        {
            var result = await client.GetPromptListAsync();
            Assert.IsFalse(result.Prompts.Count != 0);
        }

        [TestCategory("Misc")]
        [TestMethod("Test Ping")]
        public async Task TestPing()
        {
            await client.SendPingAsync();
        }

        [TestCategory("Resources")]
        [TestMethod("Resources/List")]
        public async Task TestResources()
        {
            var result = await client.GetResourcesAsync(); 
            Assert.IsTrue(result.Resources.Count != 0);
            result.Resources.ForEach(result =>
            {
                Console.WriteLine(result.Name);
            });
        }

        [TestCategory("Tools")]
        [TestMethod("Tools/Call with parameter obj")]
        public async Task TestCallToolWithParameterObject()
        {
            var result = await client.CallToolAsync("AddComplex", new Dictionary<string, object> { { "obj", new ComplicatedObject { Name = "Claude", Age = 25, Hobbies = ["Programming", "Gaming"] } } });
            string response = result.Content[0].Text;
            Assert.AreEqual("Name: Claude, Age: 25, Hobbies: Programming, Gaming", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }
    }
}
