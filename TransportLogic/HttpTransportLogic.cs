using System.Text.Json.Nodes;

namespace utcp
{
    public class HttpTransportLogic : ITransport
    {
        private readonly UtcpClientConfig _config;

        public HttpTransportLogic(UtcpClientConfig config)
        {
            _config = config;
        }

        public JsonNode Execute(Transport transport, JsonObject inputs)
        {
            var httpTransport = (HttpTransport)transport;
            using var client = new System.Net.Http.HttpClient();

            if (httpTransport.Auth != null)
            {
                if (httpTransport.Auth.AuthType == "api_key")
                {
                    var apiKey = ResolveVariable(httpTransport.Auth.ApiKey);
                    if (httpTransport.Auth.VarName != null)
                    {
                        client.DefaultRequestHeaders.Add(httpTransport.Auth.VarName, apiKey);
                    }
                }
                else
                {
                    var token = ResolveVariable(httpTransport.Auth.Token);
                    var scheme = httpTransport.Auth.Scheme ?? "Bearer";
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
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
                response = client.GetAsync(uriBuilder.ToString()).Result;
            }
            else if (httpTransport.HttpMethod.ToUpper() == "POST")
            {
                var content = new System.Net.Http.StringContent(inputs.ToString(), System.Text.Encoding.UTF8, "application/json");
                response = client.PostAsync(url, content).Result;
            }
            else
            {
                throw new System.NotSupportedException($"HTTP method '{httpTransport.HttpMethod}' is not supported.");
            }

            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync().Result;
            return JsonNode.Parse(responseBody) ?? throw new System.Text.Json.JsonException("Failed to parse response from HTTP execution.");
        }

        private string? ResolveVariable(string? value)
        {
            return _config.ResolveVariable(value);
        }
    }
}
