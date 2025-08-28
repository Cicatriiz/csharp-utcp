// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Substitution;

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class DefaultVariableSubstitutor : IVariableSubstitutor
{
    private static readonly Regex VariableRegex = new("\n?\\$\\{?(?<key>[A-Za-z0-9_]+)\\}?", RegexOptions.Compiled);

    public object? Substitute(object? obj, UtcpClientConfig config, string? variableNamespace = null)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is string s)
        {
            return this.ReplaceInString(s, config, variableNamespace);
        }

        if (obj is JsonNode node)
        {
            return this.Substitute(node.ToJsonString(), config, variableNamespace);
        }

        if (obj is IEnumerable<KeyValuePair<string, object?>> dict)
        {
            var result = new Dictionary<string, object?>();
            foreach (var (k, v) in dict)
            {
                result[k] = this.Substitute(v, config, variableNamespace);
            }
            return result;
        }

        if (obj is IEnumerable<object?> list)
        {
            return list.Select(item => this.Substitute(item, config, variableNamespace)).ToList();
        }

        return obj;
    }

    public IReadOnlyList<string> FindRequiredVariables(object? obj, string? variableNamespace = null)
    {
        var set = new HashSet<string>();
        this.CollectVariables(obj, set, variableNamespace);
        return set.ToList();
    }

    private string ReplaceInString(string input, UtcpClientConfig config, string? variableNamespace)
    {
        return VariableRegex.Replace(input, match =>
        {
            var key = match.Groups["key"].Value;
            var namespaced = ApplyNamespace(key, variableNamespace);
            if (config.Variables.TryGetValue(namespaced, out var value))
            {
                return value;
            }

            if (config.LoadVariablesFrom is not null)
            {
                foreach (var loader in config.LoadVariablesFrom)
                {
                    var loaded = loader.Get(namespaced);
                    if (!string.IsNullOrEmpty(loaded))
                    {
                        return loaded!;
                    }
                }
            }

            var env = Environment.GetEnvironmentVariable(namespaced);
            if (!string.IsNullOrEmpty(env))
            {
                return env!;
            }

            throw new Utcp.Core.Interfaces.UtcpVariableNotFound(namespaced);
        });
    }

    private static string ApplyNamespace(string key, string? variableNamespace)
    {
        if (string.IsNullOrEmpty(variableNamespace))
        {
            return key;
        }

        var ns = variableNamespace!.Replace("_", "!").Replace("!", "__");
        return ns + "_" + key;
    }

    private void CollectVariables(object? obj, HashSet<string> into, string? variableNamespace)
    {
        if (obj is null)
        {
            return;
        }

        if (obj is string s)
        {
            foreach (Match m in VariableRegex.Matches(s))
            {
                var key = m.Groups["key"].Value;
                into.Add(ApplyNamespace(key, variableNamespace));
            }
            return;
        }

        if (obj is IEnumerable<KeyValuePair<string, object?>> dict)
        {
            foreach (var kvp in dict)
            {
                this.CollectVariables(kvp.Value, into, variableNamespace);
            }
            return;
        }

        if (obj is IEnumerable<object?> list)
        {
            foreach (var item in list)
            {
                this.CollectVariables(item, into, variableNamespace);
            }
        }
    }
}

