using System.Text.Json.Serialization;

namespace utcp
{
    [JsonConverter(typeof(AuthConverter))]
    public abstract class Auth
    {
        [JsonPropertyName("auth_type")]
        public required string AuthType { get; set; }
    }

    public class ApiKeyAuth : Auth
    {
        [JsonPropertyName("api_key")]
        public required string ApiKey { get; set; }

        [JsonPropertyName("var_name")]
        public string VarName { get; set; } = "X-Api-Key";

        [JsonPropertyName("location")]
        public string Location { get; set; } = "header";
    }

    public class BasicAuth : Auth
    {
        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }

    public class OAuth2Auth : Auth
    {
        [JsonPropertyName("token_url")]
        public required string TokenUrl { get; set; }

        [JsonPropertyName("client_id")]
        public required string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public required string ClientSecret { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
