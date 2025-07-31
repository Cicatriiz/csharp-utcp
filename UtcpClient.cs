using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace utcp
{
    public class UtcpClient
    {
        private readonly IToolRepository _toolRepository;
        private readonly IToolSearchStrategy _toolSearchStrategy;
        private readonly UtcpClientConfig _config;

        public UtcpClient(IToolRepository? toolRepository = null, IToolSearchStrategy? toolSearchStrategy = null, UtcpClientConfig? config = null)
        {
            _toolRepository = toolRepository ?? new InMemToolRepository();
            _toolSearchStrategy = toolSearchStrategy ?? new TagSearch();
            _config = config ?? new UtcpClientConfig();
        }

        public void LoadManual(string path)
        {
            var json = File.ReadAllText(path);
            var manual = JsonSerializer.Deserialize<UTCPManual>(json);
            if (manual == null) throw new JsonException("Failed to deserialize UTCP manual.");
            if (manual.Tools != null)
            {
                foreach (var tool in manual.Tools)
                {
                    _toolRepository.AddTool(tool);
                }
            }
        }

        public void LoadOpenApi(string openApiSpec)
        {
            var manual = OpenApiConverter.Convert(openApiSpec);
            if (manual.Tools != null)
            {
                foreach (var tool in manual.Tools)
                {
                    _toolRepository.AddTool(tool);
                }
            }
        }

        public IEnumerable<Tool> SearchTools(string query)
        {
            return _toolSearchStrategy.Search(_toolRepository.GetAllTools(), query);
        }

        public JsonNode ExecuteTool(string toolName, JsonObject inputs)
        {
            var tool = _toolRepository.GetTool(toolName);
            if (tool == null)
            {
                throw new KeyNotFoundException($"Tool '{toolName}' not found.");
            }

            if (tool.ToolTransport == null)
            {
                throw new InvalidOperationException($"Tool '{toolName}' has no transport defined.");
            }

            ITransport transportLogic = tool.ToolTransport switch
            {
                HttpTransport => new HttpTransportLogic(_config),
                CliTransport => new CliTransportLogic(_config),
                TextTransport => new TextTransportLogic(),
                GraphQLTransport => new GraphQLTransportLogic(_config),
                GrpcTransport => new GrpcTransportLogic(),
                TcpTransport => new TcpTransportLogic(),
                UdpTransport => new UdpTransportLogic(),
                _ => throw new NotSupportedException($"Transport type '{tool.ToolTransport.TransportType}' is not supported for non-streaming execution.")
            };

            return transportLogic.Execute(tool.ToolTransport, inputs);
        }

        public IAsyncEnumerable<JsonNode> ExecuteStream(string toolName, JsonObject inputs)
        {
            var tool = _toolRepository.GetTool(toolName);
            if (tool == null)
            {
                throw new KeyNotFoundException($"Tool '{toolName}' not found.");
            }

            if (tool.ToolTransport == null)
            {
                throw new NotSupportedException($"Tool '{toolName}' has no transport defined.");
            }

            if (tool.ToolTransport is IStreamableTransport streamableTransport)
            {
                return streamableTransport.Stream(inputs);
            }

            throw new NotSupportedException($"Transport type '{tool.ToolTransport.TransportType}' is not streamable.");
        }
    }
}
