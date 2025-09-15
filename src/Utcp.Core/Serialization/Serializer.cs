// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using Utcp.Core.Models.Serialization;

public abstract class Serializer<T>
{
    public abstract T ValidateDictionary(IReadOnlyDictionary<string, object?> obj);

    public abstract Dictionary<string, object?> ToDictionary(T obj);

    public virtual T Copy(T obj)
    {
        return this.ValidateDictionary(this.ToDictionary(obj));
    }

    protected static class Runtime
    {
        internal static readonly JsonSerializerOptions Options;

        static Runtime()
        {
            Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            Options.Converters.Add(new CallTemplateJsonConverter());
            Options.Converters.Add(new AuthJsonConverter());
        }

        public static Dictionary<string, object?> ToDictionary<TValue>(TValue value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var element = JsonSerializer.SerializeToElement(value, value.GetType(), Options);
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Expected JSON object when converting to dictionary.");
            }

            return (Dictionary<string, object?>)ConvertElement(element)!;
        }

        public static TTarget Deserialize<TTarget>(IReadOnlyDictionary<string, object?> data)
        {
            var json = JsonSerializer.Serialize(data);
            var result = JsonSerializer.Deserialize<TTarget>(json, Options);
            if (result is null)
            {
                throw new InvalidOperationException("Unable to deserialize dictionary into target type.");
            }

            return result;
        }

        public static object? Deserialize(Type type, IReadOnlyDictionary<string, object?> data)
        {
            var json = JsonSerializer.Serialize(data);
            return JsonSerializer.Deserialize(json, type, Options);
        }

        private static object? ConvertElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => ConvertObject(element),
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => ConvertNumber(element),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => element.GetRawText(),
            };
        }

        private static Dictionary<string, object?> ConvertObject(JsonElement element)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var property in element.EnumerateObject())
            {
                dict[property.Name] = ConvertElement(property.Value);
            }

            return dict;
        }

        private static object ConvertNumber(JsonElement element)
        {
            if (element.TryGetInt64(out var l))
            {
                return l;
            }

            if (element.TryGetDouble(out var d))
            {
                return d;
            }

            return element.GetDecimal();
        }
    }
}
