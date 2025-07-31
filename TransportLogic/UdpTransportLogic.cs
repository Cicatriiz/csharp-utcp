using System.Net;
using System.Text.Json.Nodes;

namespace utcp
{
    public class UdpTransportLogic : ITransport
    {
        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var udpTransport = (UdpTransport)transport;
            using var client = new System.Net.Sockets.UdpClient();
            var bytes = System.Text.Encoding.UTF8.GetBytes(inputs.ToString());
            client.Send(bytes, bytes.Length, udpTransport.Host, udpTransport.Port);
            var remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var receivedBytes = client.Receive(ref remoteEP);
            var response = System.Text.Encoding.UTF8.GetString(receivedBytes);
            return JsonNode.Parse(response) ?? throw new System.Text.Json.JsonException("Failed to parse response from UDP execution.");
        }
    }
}
