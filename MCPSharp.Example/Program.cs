using MCPSharp;

await MCPServer.StartAsync("TestServer", "1.0");


namespace MCPSharp.Example
{
    ///<summary>testing interface for custom .net mcp server</summary>
    [McpTool]
    public class MCPDev()
    {
        ///<summary>just returns a message for testing.</summary>
        [McpFunction]
        public static string Hello() => "hello, claude.";

        ///<summary>returns ths input string back</summary>
        ///<param name="input">the string to echo</param>
        [McpFunction]
        public static string Echo([McpParameter(true)] string input) => input;

        ///<summary>Add Two Numbers</summary>
        ///<param name="a">first number</param>
        ///<param name="b">second number</param>
        [McpFunction]
        public static string Add(int a, int b) => (a + b).ToString();


        /// <summary>
        /// Adds a complex object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [McpFunction]
        public static string AddComplex(ComplicatedObject obj) => $"Name: {obj.Name}, Age: {obj.Age}, Hobbies: {string.Join(", ", obj.Hobbies)}";

        [McpFunction("throw_exception", "throws an exception - for ensuring we handle them gracefully")]
        public static string Exception() => throw new Exception("This is an exception");
    }

    /// <summary>
    /// A complicated object
    /// </summary>
    public class ComplicatedObject()
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Hobbies { get; set; }
    }
}