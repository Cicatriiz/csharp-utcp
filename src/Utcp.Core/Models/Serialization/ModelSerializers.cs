// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models.Serialization;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed class AuthSerializer : Serializer<Auth>
{
    public override Auth ValidateDictionary(IReadOnlyDictionary<string, object?> obj)
    {
        if (!obj.TryGetValue("auth_type", out var discriminator) || discriminator is not string authType)
        {
            throw new ArgumentException("Dictionary does not contain an auth_type discriminator.", nameof(obj));
        }

        if (!PolymorphicRegistry.TryGetAuthType(authType, out var type) || type is null)
        {
            throw new InvalidOperationException($"Unknown Auth type: {authType}");
        }

        if (Runtime.Deserialize(type, obj) is not Auth auth)
        {
            throw new InvalidOperationException($"Unable to deserialize Auth type '{authType}'.");
        }

        return auth;
    }

    public override Dictionary<string, object?> ToDictionary(Auth obj)
    {
        return Runtime.ToDictionary(obj);
    }
}

public sealed class CallTemplateSerializer : Serializer<CallTemplate>
{
    public override CallTemplate ValidateDictionary(IReadOnlyDictionary<string, object?> obj)
    {
        if (!obj.TryGetValue("call_template_type", out var discriminator) || discriminator is not string templateType)
        {
            throw new ArgumentException("Dictionary does not contain a call_template_type discriminator.", nameof(obj));
        }

        if (!PolymorphicRegistry.TryGetCallTemplateType(templateType, out var type) || type is null)
        {
            throw new InvalidOperationException($"Unknown CallTemplate type: {templateType}");
        }

        if (Runtime.Deserialize(type, obj) is not CallTemplate template)
        {
            throw new InvalidOperationException($"Unable to deserialize CallTemplate type '{templateType}'.");
        }

        return template;
    }

    public override Dictionary<string, object?> ToDictionary(CallTemplate obj)
    {
        return Runtime.ToDictionary(obj);
    }
}

public sealed class JsonSchemaSerializer : Serializer<JsonSchema>
{
    public override JsonSchema ValidateDictionary(IReadOnlyDictionary<string, object?> obj)
    {
        return Runtime.Deserialize<JsonSchema>(obj);
    }

    public override Dictionary<string, object?> ToDictionary(JsonSchema obj)
    {
        return Runtime.ToDictionary(obj);
    }
}

public sealed class ToolSerializer : Serializer<Tool>
{
    public override Tool ValidateDictionary(IReadOnlyDictionary<string, object?> obj)
    {
        return Runtime.Deserialize<Tool>(obj);
    }

    public override Dictionary<string, object?> ToDictionary(Tool obj)
    {
        return Runtime.ToDictionary(obj);
    }
}

public sealed class UtcpManualSerializer : Serializer<UtcpManual>
{
    public override UtcpManual ValidateDictionary(IReadOnlyDictionary<string, object?> obj)
    {
        return Runtime.Deserialize<UtcpManual>(obj);
    }

    public override Dictionary<string, object?> ToDictionary(UtcpManual obj)
    {
        return Runtime.ToDictionary(obj);
    }
}
