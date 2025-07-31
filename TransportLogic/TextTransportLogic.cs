using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public class TextTransportLogic : ITransport
    {
        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var textTransport = (TextTransport)transport;
            var path = textTransport.Path;
            foreach (var input in inputs)
            {
                path = path.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? string.Empty);
            }
            var content = await File.ReadAllTextAsync(path);
            return JsonNode.Parse(content) ?? throw new System.Text.Json.JsonException("Failed to parse response from Text execution.");
        }
    }
}
