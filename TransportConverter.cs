using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class TransportConverter : JsonConverter<Transport>
    {
        public override Transport Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonNode.Parse(ref reader)?.AsObject();
            if (jsonObject == null)
            {
                throw new JsonException("Invalid JSON for Transport.");
            }
            var transportType = jsonObject["transport_type"]?.GetValue<string>();

            return transportType switch
            {
                "http" => jsonObject.Deserialize<HttpTransport>(options) ?? throw new JsonException("Failed to deserialize HttpTransport."),
                "cli" => jsonObject.Deserialize<CliTransport>(options) ?? throw new JsonException("Failed to deserialize CliTransport."),
                "text" => jsonObject.Deserialize<TextTransport>(options) ?? throw new JsonException("Failed to deserialize TextTransport."),
                "sse" => jsonObject.Deserialize<SseTransport>(options) ?? throw new JsonException("Failed to deserialize SseTransport."),
                "graphql" => jsonObject.Deserialize<GraphQLTransport>(options) ?? throw new JsonException("Failed to deserialize GraphQLTransport."),
                "websocket" => jsonObject.Deserialize<WebSocketTransport>(options) ?? throw new JsonException("Failed to deserialize WebSocketTransport."),
                "grpc" => jsonObject.Deserialize<GrpcTransport>(options) ?? throw new JsonException("Failed to deserialize GrpcTransport."),
                "tcp" => jsonObject.Deserialize<TcpTransport>(options) ?? throw new JsonException("Failed to deserialize TcpTransport."),
                "udp" => jsonObject.Deserialize<UdpTransport>(options) ?? throw new JsonException("Failed to deserialize UdpTransport."),
                _ => throw new NotSupportedException($"Transport type '{transportType}' is not supported.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Transport value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
