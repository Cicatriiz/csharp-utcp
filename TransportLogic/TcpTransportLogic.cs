using System.IO;
using System.Text.Json.Nodes;

namespace utcp
{
    public class TcpTransportLogic : ITransport
    {
        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var tcpTransport = (TcpTransport)transport;
            using var client = new System.Net.Sockets.TcpClient(tcpTransport.Host, tcpTransport.Port);
            using var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            writer.Write(inputs.ToString());
            writer.Flush();
            var reader = new StreamReader(stream);
            var response = reader.ReadToEnd();
            return JsonNode.Parse(response) ?? throw new System.Text.Json.JsonException("Failed to parse response from TCP execution.");
        }
    }
}
