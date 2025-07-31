using System.Text.Json.Serialization;

namespace utcp
{
    [JsonConverter(typeof(TransportConverter))]
    public abstract class Transport
    {
        [JsonPropertyName("transport_type")]
        public required string TransportType { get; set; }
    }
}
