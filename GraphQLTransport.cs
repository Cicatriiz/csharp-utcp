using System.Text.Json.Serialization;

namespace utcp
{
    public class GraphQLTransport : Transport
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("query")]
        public required string Query { get; set; }

        [JsonPropertyName("variables")]
        public object? Variables { get; set; }

        [JsonPropertyName("operation_type")]
        public string OperationType { get; set; } = "query";

        [JsonPropertyName("operation_name")]
        public string? OperationName { get; set; }

        [JsonPropertyName("headers")]
        public System.Collections.Generic.Dictionary<string, string>? Headers { get; set; }

        [JsonPropertyName("header_fields")]
        public System.Collections.Generic.List<string>? HeaderFields { get; set; }

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
