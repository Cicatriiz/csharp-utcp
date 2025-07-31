using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public class UdpTransportLogic : ITransport
    {
        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var udpTransport = (UdpTransport)transport;
            using var client = new System.Net.Sockets.UdpClient();
            var bytes = System.Text.Encoding.UTF8.GetBytes(inputs.ToString());
            await client.SendAsync(bytes, bytes.Length, udpTransport.Host, udpTransport.Port);
            var result = await client.ReceiveAsync();
            var response = System.Text.Encoding.UTF8.GetString(result.Buffer);
            return JsonNode.Parse(response) ?? throw new System.Text.Json.JsonException("Failed to parse response from UDP execution.");
        }
    }
}
