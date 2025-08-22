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

public class HttpCommunicationProtocolTests
{
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
    }
}

