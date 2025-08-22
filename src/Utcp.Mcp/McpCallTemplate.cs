// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Mcp;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record McpCallTemplate : CallTemplate
{
    public string Transport { get; init; } = "stdio"; // or "http"
    public string? Command { get; init; }
    public string? Url { get; init; }
    public IReadOnlyDictionary<string, McpServerConfig>? Servers { get; init; }
    public McpConfig? Config { get; init; }

    public McpCallTemplate()
    {
        CallTemplateType = "mcp";
        PolymorphicRegistry.RegisterCallTemplateDerivedType("mcp", typeof(McpCallTemplate));
    }
}


