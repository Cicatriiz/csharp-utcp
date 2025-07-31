using System.Text.Json.Nodes;

namespace utcp
{
    public interface ITransport
    {
        JsonNode Execute(Transport transport, JsonObject inputs);
    }
}
