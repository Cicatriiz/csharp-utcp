// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Mcp;

using System.Text.Json.Serialization;

public sealed record McpConfig
{
    [JsonPropertyName("mcpServers")] public required IReadOnlyDictionary<string, McpServerConfig> McpServers { get; init; }
}


