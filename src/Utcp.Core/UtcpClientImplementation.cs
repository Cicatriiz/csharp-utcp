// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core;

using System.Text.Json;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;
using Utcp.Core.Repositories;
using Utcp.Core.Search;
using Utcp.Core.Substitution;

public sealed class UtcpClientImplementation : UtcpClient
{
    private readonly IVariableSubstitutor substitutor;
    private readonly IConcurrentToolRepository repository;
    private readonly IToolSearchStrategy searchStrategy;

    private UtcpClientImplementation(UtcpClientConfig config, IVariableSubstitutor substitutor, IConcurrentToolRepository repository, IToolSearchStrategy searchStrategy, string? rootDir)
        : base(config, rootDir)
    {
        this.substitutor = substitutor;
        this.repository = repository;
        this.searchStrategy = searchStrategy;
    }

    public static async Task<UtcpClient> CreateAsync(string? rootDir = null, object? config = null)
    {
        var loadedConfig = await LoadConfigAsync(config).ConfigureAwait(false);
        var client = new UtcpClientImplementation(loadedConfig, new DefaultVariableSubstitutor(), new InMemToolRepository(), new TagAndDescriptionWordMatchStrategy(), rootDir);

        // Substitute top-level variables
        if (client.Config.Variables.Count > 0)
        {
            var copy = new UtcpClientConfig
            {
                ToolRepository = client.Config.ToolRepository,
                ToolSearchStrategy = client.Config.ToolSearchStrategy,
                PostProcessing = client.Config.PostProcessing,
                ManualCallTemplates = client.Config.ManualCallTemplates,
            };
            var substituted = (Dictionary<string, string>)client.substitutor.Substitute(client.Config.Variables, copy)!;
            client.Config = client.Config with { Variables = substituted };
        }

        // Register manuals from config if any
        if (loadedConfig.ManualCallTemplates.Count > 0)
        {
            await client.RegisterManualsAsync(loadedConfig.ManualCallTemplates).ConfigureAwait(false);
        }
        return client;
    }

    public async Task<RegisterManualResult> RegisterManualAsync(CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        if (!Core.Protocols.ProtocolRegistry.TryGet(manualCallTemplate.CallTemplateType, out var protocol) || protocol is null)
        {
            throw new InvalidOperationException($"No protocol registered for type '{manualCallTemplate.CallTemplateType}'");
        }

        var result = await protocol.RegisterManualAsync(this, manualCallTemplate, cancellationToken).ConfigureAwait(false);
        await this.repository.SaveManualAsync(result.ManualCallTemplate, result.Manual, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task RegisterManualsAsync(IEnumerable<CallTemplate> manualCallTemplates, CancellationToken cancellationToken = default)
    {
        foreach (var t in manualCallTemplates)
        {
            await this.RegisterManualAsync(t, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<IReadOnlyList<Tool>> SearchToolsAsync(string query, int limit = 10, IReadOnlyList<string>? anyOfTagsRequired = null, CancellationToken cancellationToken = default)
    {
        return this.searchStrategy.SearchToolsAsync(this.repository, query, limit, anyOfTagsRequired, cancellationToken);
    }

    public async Task<object?> CallToolAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, CancellationToken cancellationToken = default)
    {
        var tools = await this.repository.GetToolsAsync(cancellationToken).ConfigureAwait(false);
        var tool = tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
        if (tool is null)
        {
            throw new KeyNotFoundException($"Tool not found: {toolName}");
        }

        var template = tool.ToolCallTemplate;
        if (!Core.Protocols.ProtocolRegistry.TryGet(template.CallTemplateType, out var protocol) || protocol is null)
        {
            throw new InvalidOperationException($"No protocol registered for type '{template.CallTemplateType}'");
        }

        var result = await protocol.CallToolAsync(this, toolName, toolArgs, template, cancellationToken).ConfigureAwait(false);

        foreach (var pp in this.Config.PostProcessing)
        {
            result = pp.PostProcess(this, tool, template, result);
        }

        return result;
    }

    public async IAsyncEnumerable<object?> CallToolStreamingAsync(string toolName, IReadOnlyDictionary<string, object?> toolArgs, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var tools = await this.repository.GetToolsAsync(cancellationToken).ConfigureAwait(false);
        var tool = tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
        if (tool is null)
        {
            throw new KeyNotFoundException($"Tool not found: {toolName}");
        }

        var template = tool.ToolCallTemplate;
        if (!Core.Protocols.ProtocolRegistry.TryGet(template.CallTemplateType, out var protocol) || protocol is null)
        {
            throw new InvalidOperationException($"No protocol registered for type '{template.CallTemplateType}'");
        }

        await foreach (var chunk in protocol.CallToolStreamingAsync(this, toolName, toolArgs, template, cancellationToken).ConfigureAwait(false))
        {
            yield return chunk; // Post-processing for streaming can be added later per requirements
        }
    }

    private static async Task<UtcpClientConfig> LoadConfigAsync(object? config)
    {
        if (config is null)
        {
            return new UtcpClientConfig
            {
                ToolRepository = new InMemToolRepository(),
                ToolSearchStrategy = new TagAndDescriptionWordMatchStrategy(),
            };
        }

        if (config is string path)
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var cfg = JsonSerializer.Deserialize<UtcpClientConfig>(json);
            if (cfg is null)
            {
                throw new InvalidOperationException("Invalid config file");
            }
            return cfg;
        }

        if (config is Dictionary<string, object?> dict)
        {
            var json = JsonSerializer.Serialize(dict);
            var cfg = JsonSerializer.Deserialize<UtcpClientConfig>(json);
            if (cfg is null)
            {
                throw new InvalidOperationException("Invalid config dictionary");
            }
            return cfg;
        }

        if (config is UtcpClientConfig typed)
        {
            return typed;
        }

        throw new ArgumentException("Unsupported config input");
    }
}

