using System.Text.Json.Serialization;

namespace utcp
{
    public class TextTransport : Transport
    {
        [JsonPropertyName("path")]
        public required string Path { get; set; }
    }
}
