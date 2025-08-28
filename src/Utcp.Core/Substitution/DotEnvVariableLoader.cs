// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Substitution;

using System.Text.RegularExpressions;
using Utcp.Core.Models;

public sealed class DotEnvVariableLoader : IVariableLoader
{
    private readonly IReadOnlyDictionary<string, string> values;

    public DotEnvVariableLoader(string? rootDirectory = null, string fileName = ".env")
    {
        var root = string.IsNullOrEmpty(rootDirectory) ? Environment.CurrentDirectory : rootDirectory!;
        var path = Path.Combine(root, fileName);
        this.values = File.Exists(path) ? Parse(File.ReadAllLines(path)) : new Dictionary<string, string>();
    }

    public string? Get(string key)
    {
        return this.values.TryGetValue(key, out var v) ? v : null;
    }

    private static IReadOnlyDictionary<string, string> Parse(string[] lines)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        var rx = new Regex(@"^\s*([A-Za-z0-9_]+)\s*=\s*(.*)\s*$", RegexOptions.Compiled);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }
            var m = rx.Match(line);
            if (!m.Success)
            {
                continue;
            }
            var k = m.Groups[1].Value;
            var v = m.Groups[2].Value;
            if ((v.StartsWith("\"") && v.EndsWith("\"")) || (v.StartsWith("'") && v.EndsWith("'")))
            {
                v = v.Substring(1, v.Length - 2);
            }
            dict[k] = v;
        }
        return dict;
    }
}


