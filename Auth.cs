using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class Auth
    {
        [JsonPropertyName("auth_type")]
        public required string AuthType { get; set; }

        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }

        [JsonPropertyName("var_name")]
        public string? VarName { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("scheme")]
        public string? Scheme { get; set; }
    }
}
