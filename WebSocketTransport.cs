using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class WebSocketTransport : Transport, IStreamableTransport
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        public async IAsyncEnumerable<JsonNode> Stream(JsonObject inputs)
        {
            using var ws = new ClientWebSocket();
            var url = new System.Uri(Url);
            await ws.ConnectAsync(url, CancellationToken.None);

            var buffer = new byte[1024 * 4];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new System.ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var jsonNode = JsonNode.Parse(str);
                    if (jsonNode != null)
                    {
                        yield return jsonNode;
                    }
                }
            }
        }
    }
}
