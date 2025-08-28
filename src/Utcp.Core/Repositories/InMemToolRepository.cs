// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Repositories;

using System.Collections.Concurrent;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class InMemToolRepository : IConcurrentToolRepository
{
    private readonly SemaphoreSlim writeLock = new(1, 1);
    private readonly ConcurrentDictionary<string, Tool> toolsByName = new();
    private readonly ConcurrentDictionary<string, UtcpManual> manualsByName = new();
    private readonly ConcurrentDictionary<string, CallTemplate> manualTemplatesByName = new();

    public async Task SaveManualAsync(CallTemplate manualCallTemplate, UtcpManual manual, CancellationToken cancellationToken = default)
    {
        await this.writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var manualName = manualCallTemplate.Name;

            if (this.manualsByName.TryGetValue(manualName, out var oldManual))
            {
                foreach (var oldTool in oldManual.Tools)
                {
                    this.toolsByName.TryRemove(oldTool.Name, out _);
                }
            }

            this.manualTemplatesByName[manualName] = manualCallTemplate;
            this.manualsByName[manualName] = manual;

            foreach (var tool in manual.Tools)
            {
                this.toolsByName[tool.Name] = tool;
            }
        }
        finally
        {
            this.writeLock.Release();
        }
    }

    public async Task<bool> RemoveManualAsync(string manualName, CancellationToken cancellationToken = default)
    {
        await this.writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!this.manualsByName.TryRemove(manualName, out var oldManual))
            {
                return false;
            }

            this.manualTemplatesByName.TryRemove(manualName, out _);

            foreach (var tool in oldManual.Tools)
            {
                this.toolsByName.TryRemove(tool.Name, out _);
            }

            return true;
        }
        finally
        {
            this.writeLock.Release();
        }
    }

    public Task<IReadOnlyList<string>> GetManualNamesAsync(CancellationToken cancellationToken = default)
    {
        var list = (IReadOnlyList<string>)this.manualsByName.Keys.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<UtcpManual>> GetManualsAsync(CancellationToken cancellationToken = default)
    {
        var list = (IReadOnlyList<UtcpManual>)this.manualsByName.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Tool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        var list = (IReadOnlyList<Tool>)this.toolsByName.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<CallTemplate>> GetManualCallTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var list = (IReadOnlyList<CallTemplate>)this.manualTemplatesByName.Values.ToList();
        return Task.FromResult(list);
    }

    public Task<UtcpManual?> TryGetManualByNameAsync(string manualName, CancellationToken cancellationToken = default)
    {
        this.manualsByName.TryGetValue(manualName, out var manual);
        return Task.FromResult(manual);
    }

    public Task<CallTemplate?> TryGetManualCallTemplateByNameAsync(string manualName, CancellationToken cancellationToken = default)
    {
        this.manualTemplatesByName.TryGetValue(manualName, out var template);
        return Task.FromResult(template);
    }
}

