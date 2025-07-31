using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public interface ITransport
    {
        Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs);
    }
}
