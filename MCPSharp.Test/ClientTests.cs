using System.Runtime.InteropServices;

namespace MCPSharp.Test
{
    [TestClass]
    public class ClientTests
    {
        public static MCPClient client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) 
        {
            client = new("Test Client", "1.0.0",
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "c:\\program files\\nodejs\\npx.cmd" : "npx",
                "-y @modelcontextprotocol/server-everything")
            { GetPermission = (Dictionary<string, object> parameters) => { return true; } };
        } 


        [ClassCleanup]
        public static void ClassCleanup() { client?.Dispose(); }

        [TestCategory("Tools")]
        [TestMethod("Client - Tools/list")]
        public async Task TestListTools()
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
        [TestMethod("Client - Tools/Call")]
        public async Task TestCallTool()
        {
            
            var result = await client.CallToolAsync("echo", new Dictionary<string, object> { { "message", "test" } });

            string response = result.Content[0].Text;
            Assert.AreEqual("Echo: test", response);
            Assert.AreEqual("text", result.Content[0].Type);

        }

        [TestCategory("Prompts")]
        [TestMethod("Client - Prompts/List")] 
        public async Task TestListPrompts()
        {
            var result = await client.GetPromptListAsync();
            Assert.IsFalse(result.Prompts.Count != 0);
        }

        [TestCategory("Resources")]
        [TestMethod("Client - Resources/List")]
        public async Task TestResources()
        {
            var result = await client.GetResourcesAsync();
            Assert.IsTrue(result.Resources.Count != 0);
            result.Resources.ForEach(result =>
            {
                Console.WriteLine(result.Name);
            });
        }
    }
}
