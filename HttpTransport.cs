using System.Text.Json.Serialization;

namespace utcp
{
    public class HttpTransport : Transport
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("http_method")]
        public required string HttpMethod { get; set; }

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = "application/json";

        [JsonPropertyName("headers")]
        public System.Collections.Generic.Dictionary<string, string>? Headers { get; set; }

        [JsonPropertyName("body_field")]
        public string? BodyField { get; set; } = "body";

        [JsonPropertyName("header_fields")]
        public System.Collections.Generic.List<string>? HeaderFields { get; set; }

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
