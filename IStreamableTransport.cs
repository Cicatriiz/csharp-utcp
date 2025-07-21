using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace csharp_utcp
{
    public interface IStreamableTransport
    {
        IAsyncEnumerable<JsonNode> Stream(JsonObject inputs);
    }
}
