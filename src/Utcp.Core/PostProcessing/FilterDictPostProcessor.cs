// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.PostProcessing;

using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class FilterDictPostProcessor : IToolPostProcessor
{
    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> allowedKeysByTool;

    public FilterDictPostProcessor(IReadOnlyDictionary<string, IReadOnlySet<string>> allowedKeysByTool)
    {
        this.allowedKeysByTool = allowedKeysByTool;
    }

    public object? PostProcess(UtcpClient caller, Tool tool, CallTemplate manualCallTemplate, object? result)
    {
        if (result is IDictionary<string, object?> dict)
        {
            if (!this.allowedKeysByTool.TryGetValue(tool.Name, out var allowed))
            {
                return result;
            }

            var filtered = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                if (allowed.Contains(kvp.Key))
                {
                    filtered[kvp.Key] = kvp.Value;
                }
            }
            return filtered;
        }

        return result;
    }
}

