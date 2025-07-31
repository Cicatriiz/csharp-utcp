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

    }
}
