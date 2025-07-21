using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class GraphQLTransport : Transport
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("query")]
        public required string Query { get; set; }

        [JsonPropertyName("variables")]
        public object? Variables { get; set; }

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
