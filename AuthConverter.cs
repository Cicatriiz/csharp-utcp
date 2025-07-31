using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace utcp
{
    public class AuthConverter : JsonConverter<Auth>
    {
        public override Auth Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
            if (jsonObject == null)
            {
                throw new JsonException("Invalid JSON for Auth.");
            }
            var authType = jsonObject["auth_type"]?.GetValue<string>();

            return authType switch
            {
                "api_key" => jsonObject.Deserialize<ApiKeyAuth>(options) ?? throw new JsonException("Failed to deserialize ApiKeyAuth."),
                "basic" => jsonObject.Deserialize<BasicAuth>(options) ?? throw new JsonException("Failed to deserialize BasicAuth."),
                "oauth2" => jsonObject.Deserialize<OAuth2Auth>(options) ?? throw new JsonException("Failed to deserialize OAuth2Auth."),
                _ => throw new NotSupportedException($"Auth type '{authType}' is not supported.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Auth value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
