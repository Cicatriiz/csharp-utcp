// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net;
using System.Net.Http;
using Utcp.Core;
using Utcp.Core.Models;
using Utcp.Mcp;
using FluentAssertions;
using Xunit;

public class McpCommunicationProtocolTests
{
    [Fact]
    public async Task RegisterManual_Http_ReturnsManual()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"tools\":[{\"name\":\"manual.hello\"}]}")
        });
        var protocol = new McpCommunicationProtocol(handler);
        var template = new McpCallTemplate { CallTemplateType = "mcp", Name = "manual", Transport = "http", Url = "http://localhost/utcp" };

        var result = await protocol.RegisterManualAsync(new DummyClient(), template);
        result.Manual.Tools.Should().ContainSingle(t => t.Name == "manual.hello");
    }

    [Fact]
    public async Task CallTool_Http_PostsAndParsesJson()
    {
        var handler = new FakeHandler(req =>
        {
            if (req.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"tools\":[{\"name\":\"manual.echo\"}]}")
                };
            }
            // POST
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true,\"echo\":{\"a\":1}}")
            };
        });

        var protocol = new McpCommunicationProtocol(handler);
        var template = new McpCallTemplate { CallTemplateType = "mcp", Name = "manual", Transport = "http", Url = "http://localhost/utcp" };

        // Optional: ensure manual registration path works first
        var _ = await protocol.RegisterManualAsync(new DummyClient(), template);

        var result = await protocol.CallToolAsync(new DummyClient(), "manual.echo", new Dictionary<string, object?> { ["a"] = 1 }, template);
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<object>();
    }

    [Theory]
    [InlineData("{\"structured_output\":{\"x\":1}}", 1)]
    [InlineData("{\"content\":[{\"text\":\"123\"}]}", 123)]
    [InlineData("{\"content\":[{\"json\":{\"y\":2}}]}", 2)]
    public async Task CallTool_Http_ProcessesStructuredAndContent(string payload, int expected)
    {
        var handler = new FakeHandler(req =>
        {
            if (req.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"tools\":[{\"name\":\"manual.echo\"}]}")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload)
            };
        });
        var protocol = new McpCommunicationProtocol(handler);
        var template = new McpCallTemplate { CallTemplateType = "mcp", Name = "manual", Transport = "http", Url = "http://localhost/utcp" };
        var _ = await protocol.RegisterManualAsync(new DummyClient(), template);
        var result = await protocol.CallToolAsync(new DummyClient(), "manual.echo", new Dictionary<string, object?>(), template);
        // Result could be boxed as JsonElement/long/double; check as string convert
        result!.ToString()!.Should().Contain(expected.ToString());
    }

    [Fact]
    public async Task RegisterManual_Http_OAuth2_AddsBearerHeader()
    {
        var sawAuth = false;
        var handler = new FakeHandler(req =>
        {
            if (req.RequestUri!.AbsoluteUri.Contains("/token"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"abc\"}")
                };
            }
            if (req.Method == HttpMethod.Get)
            {
                sawAuth = req.Headers.Authorization?.Scheme == "Bearer" && req.Headers.Authorization?.Parameter == "abc";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"tools\":[{\"name\":\"manual.hello\"}]}")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var protocol = new McpCommunicationProtocol(handler);
        var template = new McpCallTemplate
        {
            CallTemplateType = "mcp",
            Name = "manual",
            Transport = "http",
            Url = "http://localhost/utcp",
            Auth = new OAuth2Auth { AuthType = "oauth2", TokenUrl = "http://localhost/token", ClientId = "id", ClientSecret = "secret" }
        };

        var result = await protocol.RegisterManualAsync(new DummyClient(), template);
        result.Manual.Tools.Should().ContainSingle(t => t.Name == "manual.hello");
        sawAuth.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterManual_MultiServer_AggregatesTools()
    {
        var handler = new FakeHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"tools\":[{\"name\":\"manual.t1\"}]}"),
        });
        var protocol = new McpCommunicationProtocol(handler);
        var template = new McpCallTemplate
        {
            CallTemplateType = "mcp",
            Name = "manual",
            Servers = new Dictionary<string, Utcp.Mcp.McpServerConfig>
            {
                ["a"] = new() { Url = "http://localhost/a" },
                ["b"] = new() { Url = "http://localhost/b" },
            }
        };
        var result = await protocol.RegisterManualAsync(new DummyClient(), template);
        result.Manual.Tools.Should().HaveCount(2);
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

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responder;
        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => this.responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.responder(request));
        }
    }
}

