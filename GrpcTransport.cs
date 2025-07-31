using System.Text.Json.Serialization;

namespace utcp
{
    public class GrpcTransport : Transport
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("service")]
        public required string Service { get; set; }

        [JsonPropertyName("method")]
        public required string Method { get; set; }

        [JsonPropertyName("service_name")]
        public required string ServiceName { get; set; }

        [JsonPropertyName("method_name")]
        public required string MethodName { get; set; }

        [JsonPropertyName("use_ssl")]
        public bool UseSsl { get; set; } = false;

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
