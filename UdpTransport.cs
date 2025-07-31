using System.Text.Json.Serialization;

namespace utcp
{
    public class UdpTransport : Transport
    {
        [JsonPropertyName("host")]
        public required string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
