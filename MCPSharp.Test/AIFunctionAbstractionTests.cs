using MCPSharp.Model.Results;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Xml.Linq;

namespace MCPSharp.Test
{
    [TestClass]
    public class AIFunctionAbstractionTests
    {
        private static MCPClient client;
        private static IList<AIFunction> functions;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            client = new("Test Client", "1.0.0", "dotnet", "MCPSharp.Example.dll");
            functions = await client.GetFunctionsAsync();
        }

        [TestCategory("AIFunctions")]
        [TestMethod("Tools/call no parameters")]
        public async Task TestInvokingAnAIFunction()
        {
        
            var function = functions.First(f => f.Name == "Hello");
            CallToolResult result = (CallToolResult)(await function!.InvokeAsync())!;
            Assert.IsFalse(result.IsError, $"{result.Content[0].Text}");
            Assert.AreEqual("hello, claude.", result.Content[0].Text);
        }

        [TestCategory("AIFunctions")]
        [TestMethod("Tools/call with parameters")]
        public async Task TestInvokingAnAIFunctionWithParameters()
        {
            
            var function = functions.First(f => f.Name == "Echo");
            var Schema = function.JsonSchema;
            Console.WriteLine(Schema);

            AIFunctionArguments keyValuePairs = new()
            {
                { "input", "hello there" }
            };

            CallToolResult result = (CallToolResult)(await function.InvokeAsync(keyValuePairs))!;

            Assert.IsFalse(result.IsError);

            var content = result.Content[0].Text;
            Assert.AreEqual("hello there", content);
            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            client?.Dispose();
        }
    }
}
