// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Text;

using System.Text;
using Utcp.Core;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class TextCommunicationProtocol : ICommunicationProtocol
{
    public Task<RegisterManualResult> RegisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        // For text protocol, we don't auto-discover tools. Return empty manual by default.
        return Task.FromResult(new RegisterManualResult
        {
            ManualCallTemplate = manualCallTemplate,
            Manual = new UtcpManual { Tools = Array.Empty<Tool>() },
        });
    }

    public Task DeregisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<object?> CallToolAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is not TextCallTemplate textTemplate)
        {
            throw new ArgumentException("Expected TextCallTemplate");
        }

        var filePath = ResolvePath(caller, toolArgs, textTemplate);
        var encoding = ResolveEncoding(textTemplate.EncodingName);
        using var fs = File.OpenRead(filePath);
        using var reader = new StreamReader(fs, encoding, detectEncodingFromByteOrderMarks: true);
        if (textTemplate.ChunkSizeBytes > 0)
        {
            // Return up to first chunk when not streaming
            var buffer = new char[textTemplate.ChunkSizeBytes];
            var read = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            return new string(buffer, 0, read);
        }
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<object?> CallToolStreamingAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is not TextCallTemplate textTemplate)
        {
            throw new ArgumentException("Expected TextCallTemplate");
        }

        var filePath = ResolvePath(caller, toolArgs, textTemplate);
        using var stream = File.OpenRead(filePath);
        var encoding = ResolveEncoding(textTemplate.EncodingName);
        if (textTemplate.ChunkSizeBytes > 0)
        {
            var buffer = new byte[textTemplate.ChunkSizeBytes];
            int read;
            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
            {
                yield return encoding.GetString(buffer, 0, read);
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }
            }
            yield break;
        }

        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }
            yield return line;
        }
    }

    private static string ResolvePath(UtcpClient caller, IReadOnlyDictionary<string, object?> toolArgs, TextCallTemplate template)
    {
        var path = template.FilePath;
        if (toolArgs.TryGetValue("filePath", out var pathObj) && pathObj is string argPath && !string.IsNullOrWhiteSpace(argPath))
        {
            path = argPath;
        }

        var combined = System.IO.Path.IsPathRooted(path) ? path : System.IO.Path.Combine(caller.RootDir, path);
        var fullRoot = System.IO.Path.GetFullPath(caller.RootDir);
        var fullPath = System.IO.Path.GetFullPath(combined);
        if (template.EnsureUnderRoot && !fullPath.StartsWith(fullRoot, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Resolved path is outside of root directory.");
        }
        return fullPath;
    }

    private static Encoding ResolveEncoding(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Encoding.UTF8;
        }
        try
        {
            return Encoding.GetEncoding(name!);
        }
        catch
        {
            return Encoding.UTF8;
        }
    }
}


