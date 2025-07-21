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
            var jsonObject = JsonNode.Parse(ref reader).AsObject();
            var transportType = jsonObject["transport_type"]?.GetValue<string>();

            return transportType switch
            {
                "http" => jsonObject.Deserialize<HttpTransport>(options),
                "cli" => jsonObject.Deserialize<CliTransport>(options),
                "text" => jsonObject.Deserialize<TextTransport>(options),
                "sse" => jsonObject.Deserialize<SseTransport>(options),
                "graphql" => jsonObject.Deserialize<GraphQLTransport>(options),
                "websocket" => jsonObject.Deserialize<WebSocketTransport>(options),
                "grpc" => jsonObject.Deserialize<GrpcTransport>(options),
                "tcp" => jsonObject.Deserialize<TcpTransport>(options),
                "udp" => jsonObject.Deserialize<UdpTransport>(options),
                _ => throw new NotSupportedException($"Transport type '{transportType}' is not supported.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Transport value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
