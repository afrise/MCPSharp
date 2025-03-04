namespace MCPSharp
{
    /// <summary>
    /// Attribute to mark a method or property as a resource.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="uri"></param>
    /// <param name="mimeType"></param>
    /// <param name="description"></param>
    [AttributeUsage(AttributeTargets.All)]
    public class McpResourceAttribute(string name = null, string uri=null, string mimeType = null, string description = null) : Attribute
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// The description of the resource.
        /// </summary>
        public string Description { get; set; } = description;
        /// <summary>
        /// The URI of the resource.
        /// </summary>
        public string Uri { get; set; } = uri;
        /// <summary>
        /// The MIME type of the resource.
        /// </summary>
        public string MimeType { get; set; } = mimeType;
    }
}
