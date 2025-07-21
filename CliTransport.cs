using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class CliTransport : Transport
    {
        [JsonPropertyName("command")]
        public required string Command { get; set; }

        [JsonPropertyName("args")]
        public List<string> Args { get; set; } = new List<string>();
    }
}
