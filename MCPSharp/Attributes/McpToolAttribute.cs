#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace MCPSharp
{
    [AttributeUsage(AttributeTargets.Class)]
    public class McpToolAttribute(string name = null, string description = null) : Attribute
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member