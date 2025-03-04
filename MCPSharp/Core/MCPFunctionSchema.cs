//using MCPSharp.Model.Schemas;
//using System.Text.Json.Serialization;

//namespace MCPSharp
//{
//    /// <summary>
//    /// This is a wrapper class for ParameterSchemas - it includes a couple extra fields that make it work with AIFunctions
//    /// </summary>
//    public class MCPFunctionSchema
//    {
//        [JsonPropertyName("description")]
//        string Description { get; set; }

//        [JsonPropertyName("type")]
//        string Type { get; set; }

//        [JsonPropertyName("properties")]
//        Dictionary<string, ParameterSchema> Properties { get; set; }

//    }
//}