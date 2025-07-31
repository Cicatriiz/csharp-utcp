using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace utcp
{
    public class HttpTransportLogic : ITransport
    {
        private readonly UtcpClientConfig _config;

        public HttpTransportLogic(UtcpClientConfig config)
        {
            _config = config;
        }

        public async Task<JsonNode> ExecuteAsync(Transport transport, JsonObject inputs)
        {
            var httpTransport = (HttpTransport)transport;
            using var client = new System.Net.Http.HttpClient();

            if (httpTransport.Auth != null)
            {
                switch (httpTransport.Auth)
                {
                    case ApiKeyAuth apiKeyAuth:
                        var apiKey = ResolveVariable(apiKeyAuth.ApiKey);
                        if (apiKeyAuth.Location == "header")
                        {
                            client.DefaultRequestHeaders.Add(apiKeyAuth.VarName, apiKey);
                        }
                        break;
                    case BasicAuth basicAuth:
                        var basicAuthValue = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{basicAuth.Username}:{basicAuth.Password}"));
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthValue);
                        break;
                    case OAuth2Auth oAuth2Auth:
                        var token = await GetOAuth2Token(oAuth2Auth);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        break;
                }
            }

            var url = httpTransport.Url;
            System.Net.Http.HttpResponseMessage response;

            if (httpTransport.HttpMethod.ToUpper() == "GET")
            {
                var uriBuilder = new System.UriBuilder(url);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (var input in inputs)
                {
                    query[input.Key] = input.Value?.ToString();
                }
                uriBuilder.Query = query.ToString();
                response = await client.GetAsync(uriBuilder.ToString());
            }
            else if (httpTransport.HttpMethod.ToUpper() == "POST")
            {
                var content = new System.Net.Http.StringContent(inputs.ToString(), System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync(url, content);
            }
            else
            {
                throw new System.NotSupportedException($"HTTP method '{httpTransport.HttpMethod}' is not supported.");
            }

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(responseBody) ?? throw new System.Text.Json.JsonException("Failed to parse response from HTTP execution.");
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
