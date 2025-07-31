using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace utcp
{
    public class GraphQLTransportLogic : ITransport
    {
        private readonly UtcpClientConfig _config;

        public GraphQLTransportLogic(UtcpClientConfig config)
        {
            _config = config;
        }

        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var graphQLTransport = (GraphQLTransport)transport;
            var client = new GraphQLHttpClient(graphQLTransport.Url, new SystemTextJsonSerializer());

            if (graphQLTransport.Auth != null)
            {
                var token = ResolveVariable(graphQLTransport.Auth.Token);
                var scheme = graphQLTransport.Auth.Scheme ?? "Bearer";
                client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
            }

            var request = new GraphQLRequest
            {
                Query = graphQLTransport.Query,
                Variables = inputs
            };

            var response = client.SendQueryAsync<JsonObject>(request).Result;
            return response.Data!;
        }

        private string? ResolveVariable(string? value)
        {
            return _config.ResolveVariable(value);
        }
    }
}
