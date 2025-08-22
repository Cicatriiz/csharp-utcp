// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

using Utcp.Core.Interfaces;

public sealed record UtcpClientConfig
{
    public Dictionary<string, string> Variables { get; init; } = new();
    public IReadOnlyList<IVariableLoader>? LoadVariablesFrom { get; init; }
    public required IConcurrentToolRepository ToolRepository { get; init; }
    public required IToolSearchStrategy ToolSearchStrategy { get; init; }
    public IReadOnlyList<IToolPostProcessor> PostProcessing { get; init; } = Array.Empty<IToolPostProcessor>();
    public IReadOnlyList<CallTemplate> ManualCallTemplates { get; init; } = Array.Empty<CallTemplate>();
}

public interface IVariableLoader
{
    string? Get(string key);
}

