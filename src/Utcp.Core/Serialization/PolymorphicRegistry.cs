// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using System.Collections.Concurrent;
using Utcp.Core.Models;

public static class PolymorphicRegistry
{
    private static readonly ConcurrentDictionary<string, Type> CallTemplateTypes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Type> AuthTypes = new(StringComparer.OrdinalIgnoreCase);

    static PolymorphicRegistry()
    {
        // Register core auth types
        RegisterAuthDerivedType("oauth2", typeof(OAuth2Auth));
        RegisterAuthDerivedType("basic", typeof(BasicAuth));
        RegisterAuthDerivedType("api_key", typeof(ApiKeyAuth));
    }

    public static void RegisterCallTemplateDerivedType(string discriminator, Type type)
    {
        CallTemplateTypes[discriminator] = type;
    }

    public static void RegisterAuthDerivedType(string discriminator, Type type)
    {
        AuthTypes[discriminator] = type;
    }

    public static bool TryGetCallTemplateType(string discriminator, out Type? type)
    {
        return CallTemplateTypes.TryGetValue(discriminator, out type);
    }

    public static bool TryGetAuthType(string discriminator, out Type? type)
    {
        return AuthTypes.TryGetValue(discriminator, out type);
    }
}

