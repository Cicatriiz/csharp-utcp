using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace utcp
{
    public class CliTransport : Transport
    {
        [JsonPropertyName("command")]
        public required string Command { get; set; }

        [JsonPropertyName("args")]
        public List<string> Args { get; set; } = new List<string>();

        [JsonPropertyName("env_vars")]
        public System.Collections.Generic.Dictionary<string, string>? EnvVars { get; set; }

        [JsonPropertyName("working_dir")]
        public string? WorkingDir { get; set; }
    }
}
