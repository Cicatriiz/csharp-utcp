// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using System.Collections.Concurrent;
using System.Text.Json;
using Utcp.Core.Interfaces;

public static class SerializerRegistry
{
    private static readonly ConcurrentDictionary<Type, object> Serializers = new();

    public static void Register<T>(IUtcpSerializer<T> serializer, bool overrideExisting = false)
    {
        var key = typeof(T);
        if (!overrideExisting && Serializers.ContainsKey(key))
        {
            return;
        }
        Serializers[key] = serializer;
    }

    public static IUtcpSerializer<T> GetOrDefault<T>()
    {
        if (Serializers.TryGetValue(typeof(T), out var existing))
        {
            return (IUtcpSerializer<T>)existing;
        }
        return new SystemTextJsonSerializer<T>();
    }

    private sealed class SystemTextJsonSerializer<T> : IUtcpSerializer<T>
    {
        public T? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public string Serialize(T value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}


