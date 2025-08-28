// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;
using Utcp.Core.Models;
using Utcp.Core.Models.Serialization;
using FluentAssertions;
using Xunit;

public class SerializationTests
{
    [Fact]
    public void UnknownCallTemplate_ThrowsUtcpSerializerValidationError()
    {
        var json = "{" + "\"call_template_type\":\"nope\",\"name\":\"x\"" + "}";
        var act = () => JsonSerializer.Deserialize<CallTemplate>(json, new JsonSerializerOptions { Converters = { new CallTemplateJsonConverter() } });
        act.Should().Throw<Utcp.Core.Interfaces.UtcpSerializerValidationError>();
    }

    [Fact]
    public void UnknownAuth_ThrowsUtcpSerializerValidationError()
    {
        var json = "{" + "\"auth_type\":\"nope\"" + "}";
        var act = () => JsonSerializer.Deserialize<Auth>(json, new JsonSerializerOptions { Converters = { new AuthJsonConverter() } });
        act.Should().Throw<Utcp.Core.Interfaces.UtcpSerializerValidationError>();
    }
}


