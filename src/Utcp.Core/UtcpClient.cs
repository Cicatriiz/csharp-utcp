// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core;

using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public abstract class UtcpClient
{
    protected UtcpClient(UtcpClientConfig config, string? rootDir)
    {
        this.Config = config;
        this.RootDir = rootDir ?? Environment.CurrentDirectory;
    }

    public UtcpClientConfig Config { get; protected set; }

    public string RootDir { get; }

    public abstract Task<RegisterManualResult> RegisterManualAsync(CallTemplate manualCallTemplate, CancellationToken cancellationToken = default);

    public abstract Task RegisterManualsAsync(IEnumerable<CallTemplate> manualCallTemplates, CancellationToken cancellationToken = default);

    public abstract Task<bool> DeregisterManualAsync(string manualName, CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyList<Tool>> SearchToolsAsync(string query, int limit = 10, IReadOnlyList<string>? anyOfTagsRequired = null, CancellationToken cancellationToken = default);

    public abstract Task<object?> CallToolAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, CancellationToken cancellationToken = default);

    public abstract IAsyncEnumerable<object?> CallToolStreamingAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyList<string>> GetRequiredVariablesForManualAndToolsAsync(CallTemplate manualCallTemplate, CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyList<string>> GetRequiredVariablesForRegisteredToolAsync(string toolName, CancellationToken cancellationToken = default);
}

