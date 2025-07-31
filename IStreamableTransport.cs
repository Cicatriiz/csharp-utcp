using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace utcp
{
    public interface IStreamableTransport
    {
        IAsyncEnumerable<JsonNode> Stream(JsonObject inputs);
    }
}
