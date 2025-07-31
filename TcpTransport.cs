using System.Text.Json.Serialization;

namespace utcp
{
    public class TcpTransport : Transport
    {
        [JsonPropertyName("host")]
        public required string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; } = 30000;

        [JsonPropertyName("auth")]
        public Auth? Auth { get; set; }
    }
}
