namespace MCPSharp.Example
{
    ///<summary>testing interface for custom .net mcp server</summary>
    public class MCPDev()
    {
        /// <summary>
        /// example of a simple resource that returns a string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [McpResource("name", "test://{name}")]
        public static string Name(string name) => $"hello {name}";

        /// <summary>
        /// example of a simple resource that returns a string
        /// </summary>
        [McpResource("settings", "test://settings", "string", "the settings document")]
        public string Settings { get; set; } = "settings";

        /// <summary>
        /// example of a function that attempts to write to console - to ensure this does not break the stream
        /// </summary>
        /// <param name="message"></param>
        [McpTool("write-to-console", "write a string to the console")] 
        public static void WriteToConsole(string message) => Console.WriteLine(message);

        ///<summary>just returns a message for testing.</summary>
        [McpTool] 
        public static string Hello() => "hello, claude.";

        ///<summary>returns ths input string back</summary>
        ///<param name="input">the string to echo</param>
        [McpTool]
        public static string Echo([McpParameter(true)] string input) => input;

        ///<summary>Add Two Numbers</summary>
        ///<param name="a">first number</param>
        ///<param name="b">second number</param>
        [McpTool] 
        public static string Add(int a, int b) => (a + b).ToString();


        /// <summary>
        /// Adds a complex object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [McpTool]
        public static string AddComplex(ComplicatedObject obj) => $"Name: {obj.Name}, Age: {obj.Age}, Hobbies: {string.Join(", ", obj.Hobbies)}";

        /// <summary>
        /// throws an exception - for ensuring we handle them gracefully
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
#pragma warning disable CS0618 // Type or member is obsolete
        [McpFunction("throw_exception")] //leaving this one as [McpFunction] for testing purposes
#pragma warning restore CS0618 // Type or member is obsolete
        public static string Exception() => throw new Exception("This is an exception");
    }
}