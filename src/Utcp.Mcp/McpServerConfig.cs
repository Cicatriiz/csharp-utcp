// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Mcp;

public sealed record McpServerConfig
{
    // HTTP settings
    public string? Url { get; init; }
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
    public int? TimeoutSeconds { get; init; }
    public int? SseReadTimeoutSeconds { get; init; }
    public bool? TerminateOnClose { get; init; }

    // stdio settings
    public string? Command { get; init; }
    public IReadOnlyList<string>? Args { get; init; }
}


