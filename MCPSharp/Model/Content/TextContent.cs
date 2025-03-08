namespace MCPSharp.Model.Content
{
    /// <summary>
    /// This is the basic text object for a message
    /// </summary>
    public class TextContent(string text = null)
    {
        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text { get; set; } = text;
        /// <summary>
        /// The type of the content. This is always "text"
        /// </summary>
        public string Type { get; } = "text";

    }
}

