using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var graphQLTransport = (GraphQLTransport)transport;
            var client = new GraphQLHttpClient(graphQLTransport.Url, new SystemTextJsonSerializer());

            if (graphQLTransport.Auth != null)
            {
                switch (graphQLTransport.Auth)
                {
                    case ApiKeyAuth apiKeyAuth:
                        var apiKey = ResolveVariable(apiKeyAuth.ApiKey);
                        if (apiKeyAuth.Location == "header")
                        {
                            client.HttpClient.DefaultRequestHeaders.Add(apiKeyAuth.VarName, apiKey);
                        }
                        break;
                    case BasicAuth basicAuth:
                        var basicAuthValue = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{basicAuth.Username}:{basicAuth.Password}"));
                        client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthValue);
                        break;
                    case OAuth2Auth oAuth2Auth:
                        var token = await GetOAuth2Token(oAuth2Auth);
                        client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        break;
                }
            }

            var request = new GraphQLRequest
            {
                Query = graphQLTransport.Query,
                Variables = inputs
            };

            var response = await client.SendQueryAsync<JsonNode>(request);
            return response.Data!;
        }

        private string? ResolveVariable(string? value)
        {
            return _config.ResolveVariable(value);
        }

        private async Task<string> GetOAuth2Token(OAuth2Auth auth)
        {
            using var client = new System.Net.Http.HttpClient();
            var content = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("grant_type", "client_credentials"),
                new System.Collections.Generic.KeyValuePair<string, string>("client_id", auth.ClientId),
                new System.Collections.Generic.KeyValuePair<string, string>("client_secret", auth.ClientSecret),
                new System.Collections.Generic.KeyValuePair<string, string>("scope", auth.Scope ?? "")
            });
            var response = await client.PostAsync(auth.TokenUrl, content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var json = System.Text.Json.JsonDocument.Parse(responseBody);
            return json.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
