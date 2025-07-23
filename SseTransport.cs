using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace csharp_utcp
{
    public class SseTransport : Transport, IStreamableTransport
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        public async IAsyncEnumerable<JsonNode> Stream(JsonObject inputs)
        {
            using var client = new HttpClient();
            var url = Url;
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    url = url.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? string.Empty);
                }
            }

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null && line.StartsWith("data:"))
                {
                    var data = line.Substring(5).Trim();
                    var jsonNode = JsonNode.Parse(data);
                    if (jsonNode != null)
                    {
                        yield return jsonNode;
                    }
                }
            }
        }
    }
}
