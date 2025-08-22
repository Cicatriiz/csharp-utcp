// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed class CallTemplateJsonConverter : JsonConverter<CallTemplate>
{
    public override CallTemplate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty("call_template_type", out var typeProp))
        {
            throw new JsonException("Missing call_template_type discriminator");
        }

        var discriminator = typeProp.GetString() ?? string.Empty;
        if (!PolymorphicRegistry.TryGetCallTemplateType(discriminator, out var type))
        {
            throw new JsonException($"Unknown CallTemplate type: {discriminator}");
        }

        var json = doc.RootElement.GetRawText();
        return (CallTemplate?)JsonSerializer.Deserialize(json, type!, options);
    }

    public override void Write(Utf8JsonWriter writer, CallTemplate value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}

public sealed class AuthJsonConverter : JsonConverter<Auth>
{
    public override Auth? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty("auth_type", out var typeProp))
        {
            throw new JsonException("Missing auth_type discriminator");
        }

        var discriminator = typeProp.GetString() ?? string.Empty;
        if (!PolymorphicRegistry.TryGetAuthType(discriminator, out var type))
        {
            throw new JsonException($"Unknown Auth type: {discriminator}");
        }

        var json = doc.RootElement.GetRawText();
        return (Auth?)JsonSerializer.Deserialize(json, type!, options);
    }

    public override void Write(Utf8JsonWriter writer, Auth value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}

