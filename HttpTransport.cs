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

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
