using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class TextTransport : Transport
    {
        [JsonPropertyName("path")]
        public required string Path { get; set; }
    }
}
