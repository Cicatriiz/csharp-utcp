using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public class CliTransportLogic : ITransport
    {
        private readonly UtcpClientConfig _config;

        public CliTransportLogic(UtcpClientConfig config)
        {
            _config = config;
        }

        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var cliTransport = (CliTransport)transport;
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cliTransport.Command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            if (cliTransport.Args != null)
            {
                foreach (var arg in cliTransport.Args)
                {
                    var processedArg = arg;
                    foreach (var input in inputs)
                    {
                        processedArg = processedArg.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? string.Empty);
                    }
                    var resolvedArg = ResolveVariable(processedArg);
                    if (resolvedArg != null)
                    {
                        process.StartInfo.ArgumentList.Add(resolvedArg);
                    }
                }
            }

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return JsonNode.Parse(output) ?? throw new System.Text.Json.JsonException("Failed to parse response from CLI execution.");
        }

        private string? ResolveVariable(string? value)
        {
            return _config.ResolveVariable(value);
        }
    }
}
