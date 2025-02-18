namespace MCPSharp
{
    /// <summary>
    /// Attribute to mark a class as an MCP tool. 
    /// </summary>
    /// <param name="name">The name of the tool. If not provided, the class name will be used.</param>
    /// <param name="description">A description of the tool.</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class McpFunctionAttribute(string name = null, string description = null) : Attribute
    {
        /// <summary>
        /// The name of the tool. If not provided, the class name will be used.
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// A description of the tool.
        /// </summary>
        public string Description { get; set; } = description;
    }
}
