using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class Tool
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("inputs")]
        public JsonObject? Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public JsonObject? Outputs { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("average_response_size")]
        public int? AverageResponseSize { get; set; }

        [JsonPropertyName("tool_transport")]
        public required Transport ToolTransport { get; set; }
    }
}
