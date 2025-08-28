// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net;
using System.Net.Http;
using RichardSzalay.MockHttp;
using Utcp.Core;
using Utcp.Core.Models;
using Utcp.Http;
using FluentAssertions;
using Xunit;
using Utcp.Http.OpenApi;

public class HttpCommunicationProtocolTests
{
    [Fact]
    public async Task RegisterManual_OpenApiJson_ConvertsToTools()
    {
        const string openApi = """
        {"openapi":"3.0.0","info":{"title":"t","version":"1.0.0"},"servers":[{"url":"https://api.example.com"}],"paths":{"/status":{"get":{"operationId":"getStatus","summary":"status"}}}}
        """;

        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "https://api.example.com/openapi.json")
            .Respond("application/json", openApi);

        var httpClientFactory = new MockFactory(mock);
        var protocol = new HttpCommunicationProtocol(httpClientFactory);

        var template = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("https://api.example.com/openapi.json"),
        };

        var result = await protocol.RegisterManualAsync(new DummyClient(), template);
        result.Success.Should().BeTrue();
        result.Manual.Tools.Should().ContainSingle(t => t.Name == "manual.getStatus");
    }

    [Fact]
    public async Task CallTool_HttpUrl_MustBeHttpsOrLocalhost()
    {
        var mock = new MockHttpMessageHandler();
        var httpClientFactory = new MockFactory(mock);
        var protocol = new HttpCommunicationProtocol(httpClientFactory);

        var template = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("http://api.example.com/"),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await protocol.CallToolAsync(new DummyClient(), "manual.tool", new Dictionary<string, object?>(), template);
        });
    }

    [Fact]
    public async Task OAuth2_Token_IsCached()
    {
        var tokenRequests = 0;
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://auth.example.com/token").Respond(_ =>
        {
            tokenRequests++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"t\"}")
            };
        });
        mock.When(HttpMethod.Get, "https://api.example.com/data").Respond("application/json", "{}");

        var httpClientFactory = new MockFactory(mock);
        var protocol = new HttpCommunicationProtocol(httpClientFactory);

        var template = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("https://api.example.com/data"),
            Auth = new OAuth2Auth { AuthType = "oauth2", TokenUrl = "https://auth.example.com/token", ClientId = "id", ClientSecret = "secret" }
        };

        var _ = await protocol.CallToolAsync(new DummyClient(), "manual.tool", new Dictionary<string, object?>(), template);
        var __ = await protocol.CallToolAsync(new DummyClient(), "manual.tool", new Dictionary<string, object?>(), template);
        tokenRequests.Should().Be(1);
    }
    [Fact]
    public async Task CallTool_BasicGet_ReturnsBody()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "https://example.com/")
            .Respond("application/json", "{\"ok\":true}");

        var httpClientFactory = new MockFactory(mock);
        var protocol = new HttpCommunicationProtocol(httpClientFactory);

        var template = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("https://example.com/"),
        };

        var result = await protocol.CallToolAsync(new DummyClient(), "manual.tool", new Dictionary<string, object?>(), template);
        result.Should().Be("{\"ok\":true}");
    }

    private sealed class MockFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler handler;
        public MockFactory(HttpMessageHandler handler) => this.handler = handler;
        public HttpClient CreateClient(string name) => new HttpClient(this.handler, disposeHandler: false);
    }

    private sealed class DummyClient : UtcpClient
    {
        public DummyClient() : base(new UtcpClientConfig{ ToolRepository = null!, ToolSearchStrategy = null! }, null){}

        public override Task<RegisterManualResult> RegisterManualAsync(CallTemplate manualCallTemplate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task RegisterManualsAsync(IEnumerable<CallTemplate> manualCallTemplates, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<bool> DeregisterManualAsync(string manualName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IReadOnlyList<Tool>> SearchToolsAsync(string query, int limit = 10, IReadOnlyList<string>? anyOfTagsRequired = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<object?> CallToolAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override async IAsyncEnumerable<object?> CallToolStreamingAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) { yield break; }
        public override Task<IReadOnlyList<string>> GetRequiredVariablesForManualAndToolsAsync(CallTemplate manualCallTemplate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public override Task<IReadOnlyList<string>> GetRequiredVariablesForRegisteredToolAsync(string toolName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}

