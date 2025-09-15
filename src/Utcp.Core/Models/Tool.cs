// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

public sealed record Tool
{
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public JsonSchema Inputs { get; init; } = new();
    public JsonSchema Outputs { get; init; } = new();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public int? AverageResponseSize { get; init; }
    public required CallTemplate ToolCallTemplate { get; init; }
}

public sealed record JsonSchema
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }
    [JsonPropertyName("$id")]
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public object? Type { get; init; }
    public Dictionary<string, JsonSchema>? Properties { get; init; }
    public object? Items { get; init; }
    public IReadOnlyList<string>? Required { get; init; }
    public IReadOnlyList<object?>? Enum { get; init; }
    public object? Const { get; init; }
    public object? Default { get; init; }
    public string? Format { get; init; }
    [JsonPropertyName("additionalProperties")]
    public object? AdditionalProperties { get; init; }
    public string? Pattern { get; init; }
    public double? Minimum { get; init; }
    public double? Maximum { get; init; }
    [JsonPropertyName("minLength")]
    public int? MinLength { get; init; }
    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; init; }
}

