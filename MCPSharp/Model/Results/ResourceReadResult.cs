using System.Text.Json.Serialization;

namespace MCPSharp.Model.Results
{
    public class ResourceReadResult
    {
        public string Uri { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MimeType { get; set; }
        public string Text { get; set; }
        public string Blob { get; set; }
    }

    public class ResourceReadResultContainer
    {
        public List<ResourceReadResult> Contents { get; set; }
    }
}