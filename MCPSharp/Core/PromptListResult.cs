using Newtonsoft.Json;

namespace MCPSharp
{
    /// <summary>
    /// the prompt list result
    /// </summary>
    public class PromptListResult
    {
        /// <summary>
        /// the prompts
        /// </summary>
        [JsonProperty("prompts")]
        public List<string> Prompts = [];
    }
}