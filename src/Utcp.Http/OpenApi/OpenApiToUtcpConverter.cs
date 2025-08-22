// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http.OpenApi;

using System.Text.Json;
using Utcp.Core.Models;

public interface IOpenApiToUtcpConverter
{
    UtcpManual FromJson(string openApiJson, string manualName);
}

public sealed class OpenApiToUtcpConverter : IOpenApiToUtcpConverter
{
    public UtcpManual FromJson(string openApiJson, string manualName)
    {
        using var doc = JsonDocument.Parse(openApiJson);
        var root = doc.RootElement;
        var baseUrl = root.TryGetProperty("servers", out var servers) && servers.ValueKind == JsonValueKind.Array && servers.GetArrayLength() > 0
            ? servers[0].GetProperty("url").GetString() ?? string.Empty
            : string.Empty;

        var tools = new List<Tool>();
        if (root.TryGetProperty("paths", out var paths) && paths.ValueKind == JsonValueKind.Object)
        {
            foreach (var pathProp in paths.EnumerateObject())
            {
                var path = pathProp.Name;
                var item = pathProp.Value;
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var opProp in item.EnumerateObject())
                {
                    var method = opProp.Name.ToUpperInvariant();
                    var operation = opProp.Value;
                    if (operation.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var opId = operation.TryGetProperty("operationId", out var opIdEl) ? opIdEl.GetString() : null;
                    var summary = operation.TryGetProperty("summary", out var sumEl) ? sumEl.GetString() : null;

                    var toolName = !string.IsNullOrWhiteSpace(opId)
                        ? opId!
                        : $"{method.ToLowerInvariant()}_{path.Trim('/').Replace('/', '_').Replace('{', '_').Replace('}', '_')}";
                    var fullName = string.IsNullOrWhiteSpace(manualName) ? toolName : $"{manualName}.{toolName}";

                    var url = BuildUrl(baseUrl, path);
                    var httpTemplate = new HttpCallTemplate
                    {
                        CallTemplateType = "http",
                        Name = manualName,
                        Method = method,
                        Url = new Uri(url, UriKind.Absolute),
                    };

                    tools.Add(new Tool
                    {
                        Name = fullName,
                        Description = summary ?? string.Empty,
                        Inputs = new JsonSchema { Type = "object" },
                        Outputs = new JsonSchema { Type = "object" },
                        Tags = Array.Empty<string>(),
                        ToolCallTemplate = httpTemplate,
                    });
                }
            }
        }

        return new UtcpManual
        {
            Tools = tools,
        };
    }

    private static string BuildUrl(string baseUrl, string path)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost";
        }
        if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "https://" + baseUrl.TrimStart('/');
        }
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }
        return baseUrl + path.TrimStart('/');
    }
}


