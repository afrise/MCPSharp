using System.Text.Json.Serialization;

namespace MCPSharp.Model.Schemas
{
    public class ParameterSchema
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("required")]
        public required bool Required { get; set; }
    }
}
