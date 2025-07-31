using System.IO;
using System.Text.Json.Nodes;

namespace utcp
{
    public class TextTransportLogic : ITransport
    {
        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var textTransport = (TextTransport)transport;
            var path = textTransport.Path;
            foreach (var input in inputs)
            {
                path = path.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? string.Empty);
            }
            var content = File.ReadAllText(path);
            return JsonNode.Parse(content) ?? throw new System.Text.Json.JsonException("Failed to parse response from Text execution.");
        }
    }
}
