using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace csharp_utcp
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
            var provider = new Provider
            {
                Name = "default",
                Tools = manual.Tools
            };
            LoadProvider(provider);
        }

        public void LoadProvider(Provider provider)
        {
            foreach (var tool in provider.Tools)
            {
                _toolRepository.AddTool(tool);
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

            return tool.ToolTransport switch
            {
                HttpTransport httpTransport => ExecuteHttp(httpTransport, inputs),
                CliTransport cliTransport => ExecuteCli(cliTransport, inputs),
                TextTransport textTransport => ExecuteText(textTransport, inputs),
                GraphQLTransport graphQLTransport => ExecuteGraphQL(graphQLTransport, inputs),
                GrpcTransport grpcTransport => ExecuteGrpc(grpcTransport, inputs),
                TcpTransport tcpTransport => ExecuteTcp(tcpTransport, inputs),
                UdpTransport udpTransport => ExecuteUdp(udpTransport, inputs),
                _ => throw new NotSupportedException($"Transport type '{tool.ToolTransport.TransportType}' is not supported for non-streaming execution.")
            };
        }

        private JsonNode ExecuteUdp(UdpTransport transport, JsonObject inputs)
        {
            using var client = new System.Net.Sockets.UdpClient();
            var bytes = System.Text.Encoding.UTF8.GetBytes(inputs.ToString());
            client.Send(bytes, bytes.Length, transport.Host, transport.Port);
            var remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
            var receivedBytes = client.Receive(ref remoteEP);
            var response = System.Text.Encoding.UTF8.GetString(receivedBytes);
            return JsonNode.Parse(response) ?? throw new JsonException("Failed to parse response from UDP execution.");
        }

        private JsonNode ExecuteTcp(TcpTransport transport, JsonObject inputs)
        {
            using var client = new System.Net.Sockets.TcpClient(transport.Host, transport.Port);
            using var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            writer.Write(inputs.ToString());
            writer.Flush();
            var reader = new StreamReader(stream);
            var response = reader.ReadToEnd();
            return JsonNode.Parse(response) ?? throw new JsonException("Failed to parse response from TCP execution.");
        }

        private JsonNode ExecuteGrpc(GrpcTransport transport, JsonObject inputs)
        {
            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(transport.Address);
            var assembly = System.Reflection.Assembly.Load("GrpcServer");
            var clientType = assembly.GetType(transport.Service);
            if (clientType == null) throw new TypeLoadException($"Service type '{transport.Service}' not found in assembly.");

            var client = System.Activator.CreateInstance(clientType, channel);
            if (client == null) throw new InvalidOperationException($"Failed to create instance of service '{transport.Service}'.");

            var method = client.GetType().GetMethod(transport.Method);
            if (method == null) throw new MissingMethodException($"Method '{transport.Method}' not found on service '{transport.Service}'.");

            var requestType = method.GetParameters()[0].ParameterType;
            var request = JsonSerializer.Deserialize(inputs.ToString(), requestType);
            var responseTask = method.Invoke(client, new object[] { request!, new Grpc.Core.CallOptions() });
            if (responseTask == null) throw new InvalidOperationException("gRPC method invocation returned null.");

            var response = ((dynamic)responseTask).Result;
            return JsonSerializer.SerializeToNode(response)!;
        }

        private JsonNode ExecuteGraphQL(GraphQLTransport transport, JsonObject inputs)
        {
            var client = new GraphQL.Client.Http.GraphQLHttpClient(transport.Url, new GraphQL.Client.Serializer.SystemTextJson.SystemTextJsonSerializer());

            if (transport.Auth != null)
            {
                var token = ResolveVariable(transport.Auth.Token);
                var scheme = transport.Auth.Scheme ?? "Bearer";
                client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
            }

            var request = new GraphQL.GraphQLRequest
            {
                Query = transport.Query,
                Variables = inputs
            };

            var response = client.SendQueryAsync<JsonObject>(request).Result;
            return response.Data!;
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

        private JsonNode ExecuteText(TextTransport transport, JsonObject inputs)
        {
            var path = transport.Path;
            foreach (var input in inputs)
            {
                path = path.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? string.Empty);
            }
            var content = File.ReadAllText(path);
            return JsonNode.Parse(content) ?? throw new JsonException("Failed to parse response from Text execution.");
        }

        private JsonNode ExecuteHttp(HttpTransport transport, JsonObject inputs)
        {
            using var client = new System.Net.Http.HttpClient();

            if (transport.Auth != null)
            {
                if (transport.Auth.AuthType == "api_key")
                {
                    var apiKey = ResolveVariable(transport.Auth.ApiKey);
                    if (transport.Auth.VarName != null)
                    {
                        client.DefaultRequestHeaders.Add(transport.Auth.VarName, apiKey);
                    }
                }
                else
                {
                    var token = ResolveVariable(transport.Auth.Token);
                    var scheme = transport.Auth.Scheme ?? "Bearer";
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
                }
            }

            var url = transport.Url;
            System.Net.Http.HttpResponseMessage response;

            if (transport.HttpMethod.ToUpper() == "GET")
            {
                var uriBuilder = new System.UriBuilder(url);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (var input in inputs)
                {
                    query[input.Key] = input.Value?.ToString();
                }
                uriBuilder.Query = query.ToString();
                response = client.GetAsync(uriBuilder.ToString()).Result;
            }
            else if (transport.HttpMethod.ToUpper() == "POST")
            {
                var content = new System.Net.Http.StringContent(inputs.ToString(), System.Text.Encoding.UTF8, "application/json");
                response = client.PostAsync(url, content).Result;
            }
            else
            {
                throw new NotSupportedException($"HTTP method '{transport.HttpMethod}' is not supported.");
            }

            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync().Result;
            return JsonNode.Parse(responseBody) ?? throw new JsonException("Failed to parse response from HTTP execution.");
        }

        private string? ResolveVariable(string? value)
        {
            if (value != null && value.StartsWith("$"))
            {
                var varName = value.Substring(1);
                if (_config.Variables.TryGetValue(varName, out var resolvedValue))
                {
                    return resolvedValue;
                }
                return System.Environment.GetEnvironmentVariable(varName);
            }
            return value;
        }

        private JsonNode ExecuteCli(CliTransport transport, JsonObject inputs)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = transport.Command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            if (transport.Args != null)
            {
                foreach (var arg in transport.Args)
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
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return JsonNode.Parse(output) ?? throw new JsonException("Failed to parse response from CLI execution.");
        }
    }
}
