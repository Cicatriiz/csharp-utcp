using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public class TcpTransportLogic : ITransport
    {
        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var tcpTransport = (TcpTransport)transport;
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(tcpTransport.Host, tcpTransport.Port);
            using var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(inputs.ToString());
            await writer.FlushAsync();
            var reader = new StreamReader(stream);
            var response = await reader.ReadToEndAsync();
            return JsonNode.Parse(response) ?? throw new System.Text.Json.JsonException("Failed to parse response from TCP execution.");
        }
    }
}
