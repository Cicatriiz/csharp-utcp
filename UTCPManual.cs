using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class UTCPManual
    {
        [JsonPropertyName("utcp_version")]
        public required string UtcpVersion { get; set; }

        [JsonPropertyName("manual_version")]
        public required string ManualVersion { get; set; }

        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; } = new List<Tool>();
    }
}
