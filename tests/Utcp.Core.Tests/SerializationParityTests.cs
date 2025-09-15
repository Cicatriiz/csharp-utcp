// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using FluentAssertions;
using Utcp.Core.Models;
using Utcp.Core.Models.Serialization;
using Utcp.Http;
using Xunit;

public sealed class SerializationParityTests
{
    [Fact]
    public void AuthSerializer_RoundTripsBuiltInTypes()
    {
        var serializer = new AuthSerializer();
        var auth = new OAuth2Auth
        {
            AuthType = "oauth2",
            TokenUrl = "https://example.com/token",
            ClientId = "client",
            ClientSecret = "secret",
            Scopes = new[] { "read", "write" },
        };

        var dict = serializer.ToDictionary(auth);
        dict.Should().ContainKey("auth_type").WhoseValue.Should().Be("oauth2");

        var roundTrip = serializer.ValidateDictionary(dict);
        roundTrip.Should().BeOfType<OAuth2Auth>().And.BeEquivalentTo(auth);
    }

    [Fact]
    public void CallTemplateSerializer_RoundTripsHttpTemplate()
    {
        var serializer = new CallTemplateSerializer();
        var template = new HttpCallTemplate
        {
            Name = "manual",
            Method = "POST",
            Url = new Uri("https://api.example.com/tool"),
            Headers = new Dictionary<string, string> { ["X-Test"] = "value" },
            Timeout = TimeSpan.FromSeconds(30),
        };

        var dict = serializer.ToDictionary(template);
        dict.Should().ContainKey("call_template_type").WhoseValue.Should().Be("http");

        var roundTrip = serializer.ValidateDictionary(dict);
        roundTrip.Should().BeOfType<HttpCallTemplate>().Which.Should().BeEquivalentTo(template);
    }

    [Fact]
    public void JsonSchemaSerializer_HandlesAliases()
    {
        var serializer = new JsonSchemaSerializer();
        var schema = new JsonSchema
        {
            Schema = "https://json-schema.org/draft/2020-12/schema",
            Id = "https://example.com/schema.json",
            Title = "Example",
            Description = "Sample schema",
            AdditionalProperties = false,
            MinLength = 1,
            MaxLength = 10,
        };

        var dict = serializer.ToDictionary(schema);
        dict.Should().ContainKey("$schema").WhoseValue.Should().Be("https://json-schema.org/draft/2020-12/schema");
        dict.Should().ContainKey("minLength").WhoseValue.Should().Be(1);
        dict.Should().ContainKey("additionalProperties").WhoseValue.Should().Be(false);

        var roundTrip = serializer.ValidateDictionary(dict);
        roundTrip.Should().BeEquivalentTo(schema);
    }

    [Fact]
    public void ToolSerializer_RoundTripsWithNestedTypes()
    {
        var callTemplate = new HttpCallTemplate
        {
            Name = "manual",
            Method = "GET",
            Url = new Uri("https://api.example.com/tool"),
        };

        var tool = new Tool
        {
            Name = "manual.echo",
            Description = "Echo",
            Tags = new[] { "utility", "sample" },
            Inputs = new JsonSchema { Title = "Input", MinLength = 1 },
            Outputs = new JsonSchema { Title = "Output" },
            ToolCallTemplate = callTemplate,
            AverageResponseSize = 42,
        };

        var serializer = new ToolSerializer();
        var dict = serializer.ToDictionary(tool);
        dict.Should().ContainKey("tool_call_template");

        var roundTrip = serializer.ValidateDictionary(dict);
        roundTrip.Should().BeEquivalentTo(tool, options => options
            .ComparingByMembers<JsonSchema>());
    }

    [Fact]
    public void UtcpManualSerializer_RoundTripsManual()
    {
        var callTemplate = new HttpCallTemplate
        {
            Name = "manual",
            Url = new Uri("https://api.example.com/tool"),
        };

        var tool = new Tool
        {
            Name = "manual.echo",
            ToolCallTemplate = callTemplate,
        };

        var manual = new UtcpManual
        {
            UtcpVersion = "1.0.1",
            ManualVersion = "2.0.0",
            Tools = new[] { tool },
        };

        var serializer = new UtcpManualSerializer();
        var dict = serializer.ToDictionary(manual);
        dict.Should().ContainKey("utcp_version").WhoseValue.Should().Be("1.0.1");

        var roundTrip = serializer.ValidateDictionary(dict);
        roundTrip.Should().BeEquivalentTo(manual, options => options.ComparingByMembers<Tool>());
    }
}
