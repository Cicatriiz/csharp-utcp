// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Mcp;

using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Globalization;
using Utcp.Core;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;
using Utcp.Http;

public sealed class McpCommunicationProtocol : ICommunicationProtocol
{
    private readonly HttpClient httpClient;
    private readonly ConcurrentDictionary<string, string> oauthTokens = new();

    public McpCommunicationProtocol(HttpMessageHandler? handler = null)
    {
        this.httpClient = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
    }

    public async Task<RegisterManualResult> RegisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        if (manualCallTemplate is not McpCallTemplate mcpTemplate)
        {
            throw new ArgumentException("Expected McpCallTemplate");
        }

        UtcpManual manual;
        var serversForRegister = mcpTemplate.Servers ?? mcpTemplate.Config?.McpServers;
        if (serversForRegister is { Count: > 0 })
        {
            // Iterate configured servers to find tools
            var tools = new List<Tool>();
            var errors = new List<string>();
            foreach (var (serverName, server) in serversForRegister)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(server.Url))
                    {
                        using var req = new HttpRequestMessage(HttpMethod.Get, server.Url);
                        if (server.Headers is { Count: > 0 })
                        {
                            foreach (var kvp in server.Headers)
                            {
                                req.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                            }
                        }
                        if (mcpTemplate.Auth is OAuth2Auth oauth)
                        {
                            var token = await this.GetOAuth2TokenAsync(oauth, cancellationToken).ConfigureAwait(false);
                            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        var resp = await this.httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
                        resp.EnsureSuccessStatusCode();
                        var json = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        var parsed = ParseManualJson(json, mcpTemplate.Name);
                        tools.AddRange(parsed.Tools);
                    }
                    else if (!string.IsNullOrWhiteSpace(server.Command))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = server.Command,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        using var p = new Process { StartInfo = psi };
                        if (!p.Start())
                        {
                            errors.Add($"Failed to start MCP process for server '{serverName}'");
                            continue;
                        }
                        var output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                        await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                        var parsed = ParseManualJson(output, mcpTemplate.Name);
                        tools.AddRange(parsed.Tools);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{serverName}: {ex.Message}");
                }
            }
            return new RegisterManualResult { ManualCallTemplate = manualCallTemplate, Manual = new UtcpManual { Tools = tools }, Success = errors.Count == 0, Errors = errors };
        }
        else if (string.Equals(mcpTemplate.Transport, "http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(mcpTemplate.Url))
            {
                throw new ArgumentException("MCP http transport requires Url");
            }
            using var req = new HttpRequestMessage(HttpMethod.Get, mcpTemplate.Url);
            if (mcpTemplate.Auth is OAuth2Auth oauth)
            {
                var token = await this.GetOAuth2TokenAsync(oauth, cancellationToken).ConfigureAwait(false);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var resp = await this.httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            manual = ParseManualJson(json, mcpTemplate.Name);
        }
        else if (string.Equals(mcpTemplate.Transport, "stdio", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(mcpTemplate.Command))
            {
                throw new ArgumentException("MCP stdio transport requires Command");
            }
            var psi = new ProcessStartInfo
            {
                FileName = mcpTemplate.Command,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = new Process { StartInfo = psi };
            if (!p.Start())
            {
                throw new InvalidOperationException("Failed to start MCP process");
            }
            var output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            manual = ParseManualJson(output, mcpTemplate.Name);
        }
        else
        {
            manual = new UtcpManual { Tools = Array.Empty<Tool>() };
        }

        return new RegisterManualResult
        {
            ManualCallTemplate = manualCallTemplate,
            Manual = manual,
        };
    }

    public Task DeregisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<object?> CallToolAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is not McpCallTemplate mcpTemplate)
        {
            throw new ArgumentException("Expected McpCallTemplate");
        }

        var serversForCall = mcpTemplate.Servers ?? mcpTemplate.Config?.McpServers;
        if (serversForCall is { Count: > 0 })
        {
            // Try each server until success
            foreach (var (_, server) in serversForCall)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(server.Url))
                    {
                        var requestBody = new McpCallRequest
                        {
                            ToolName = toolName,
                            Arguments = toolArgs,
                        };
                        var json = JsonSerializer.Serialize(requestBody);
                        using var req = new HttpRequestMessage(HttpMethod.Post, server.Url)
                        {
                            Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        };
                        if (server.Headers is { Count: > 0 })
                        {
                            foreach (var kvp in server.Headers)
                            {
                                req.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                            }
                        }
                        if (mcpTemplate.Auth is OAuth2Auth oauth)
                        {
                            var token = await this.GetOAuth2TokenAsync(oauth, cancellationToken).ConfigureAwait(false);
                            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        var resp = await this.httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
                        resp.EnsureSuccessStatusCode();
                        var respText = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        return ProcessToolResult(TryDeserializeToClosest(respText));
                    }
                    else if (!string.IsNullOrWhiteSpace(server.Command))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = server.Command,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        using var p = new Process { StartInfo = psi };
                        if (!p.Start())
                        {
                            continue;
                        }
                        var rpc = new JsonRpcRequest
                        {
                            Jsonrpc = "2.0",
                            Id = Guid.NewGuid().ToString("N"),
                            Method = "tools/call",
                            Params = new Dictionary<string, object?>
                            {
                                ["name"] = toolName,
                                ["arguments"] = toolArgs,
                            },
                        };
                        await p.StandardInput.WriteLineAsync(JsonSerializer.Serialize(rpc)).ConfigureAwait(false);
                        await p.StandardInput.FlushAsync().ConfigureAwait(false);
                        p.StandardInput.Close();
                        var output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                        await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                        var parsed = TryDeserializeToClosest(output);
                        return ProcessToolResult(parsed);
                    }
                }
                catch
                {
                    // try next server
                }
            }
            throw new InvalidOperationException("Tool call failed on all configured MCP servers");
        }
        else if (string.Equals(mcpTemplate.Transport, "http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(mcpTemplate.Url))
            {
                throw new ArgumentException("MCP http transport requires Url");
            }

            var requestBody = new McpCallRequest
            {
                ToolName = toolName,
                Arguments = toolArgs,
            };
            var json = JsonSerializer.Serialize(requestBody);
            using var req = new HttpRequestMessage(HttpMethod.Post, mcpTemplate.Url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            if (mcpTemplate.Auth is OAuth2Auth oauth)
            {
                var token = await this.GetOAuth2TokenAsync(oauth, cancellationToken).ConfigureAwait(false);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var resp = await this.httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var respText = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return ProcessToolResult(TryDeserializeToClosest(respText));
        }
        else if (string.Equals(mcpTemplate.Transport, "stdio", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(mcpTemplate.Command))
            {
                throw new ArgumentException("MCP stdio transport requires Command");
            }

            var psi = new ProcessStartInfo
            {
                FileName = mcpTemplate.Command,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = new Process { StartInfo = psi };
            if (!p.Start())
            {
                throw new InvalidOperationException("Failed to start MCP process");
            }

            var rpc = new JsonRpcRequest
            {
                Jsonrpc = "2.0",
                Id = Guid.NewGuid().ToString("N"),
                Method = "tools/call",
                Params = new Dictionary<string, object?>
                {
                    ["name"] = toolName,
                    ["arguments"] = toolArgs,
                },
            };
            await p.StandardInput.WriteLineAsync(JsonSerializer.Serialize(rpc)).ConfigureAwait(false);
            await p.StandardInput.FlushAsync().ConfigureAwait(false);
            p.StandardInput.Close();

            var output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var parsed = TryDeserializeToClosest(output);
            return ProcessToolResult(parsed);
        }

        throw new NotSupportedException($"Unsupported MCP transport: {mcpTemplate.Transport}");
    }

    public async IAsyncEnumerable<object?> CallToolStreamingAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is McpCallTemplate mcp)
        {
            var serversForStream = mcp.Servers ?? mcp.Config?.McpServers;
            if (serversForStream is { Count: > 0 })
            {
                foreach (var (_, server) in serversForStream)
                {
                    if (!string.IsNullOrWhiteSpace(server.Url))
                    {
                        // Attempt SSE: POST and read as stream of lines (data: ...)
                        var requestBody = new McpCallRequest { ToolName = toolName, Arguments = toolArgs };
                        var json = JsonSerializer.Serialize(requestBody);
                        using var req = new HttpRequestMessage(HttpMethod.Post, server.Url)
                        {
                            Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        };
                        if (server.Headers is { Count: > 0 })
                        {
                            foreach (var kvp in server.Headers)
                            {
                                req.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                            }
                        }
                        if (mcp.Auth is OAuth2Auth oauth)
                        {
                            var token = await this.GetOAuth2TokenAsync(oauth, cancellationToken).ConfigureAwait(false);
                            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                        using var resp = await this.httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                        resp.EnsureSuccessStatusCode();
                        await foreach (var evt in Http.SseReader.ReadAsync(resp, cancellationToken).ConfigureAwait(false))
                        {
                            if (string.IsNullOrWhiteSpace(evt.Data)) continue;
                            var payload = TryDeserializeToClosest(evt.Data);
                            if (payload is null) continue;
                            var shaped = ProcessToolResult(payload);
                            if (shaped is null) continue;
                            yield return shaped;
                        }
                        yield break;
                    }
                }
            }
        }

        var result = await this.CallToolAsync(caller, toolName, toolArgs, toolCallTemplate, cancellationToken).ConfigureAwait(false);
        yield return result;
    }

    private static UtcpManual ParseManualJson(string json, string manualName)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var tools = new List<Tool>();
        if (root.TryGetProperty("tools", out var toolsEl) && toolsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var t in toolsEl.EnumerateArray())
            {
                string? name = null;
                string? description = null;
                if (t.ValueKind == JsonValueKind.Object)
                {
                    if (t.TryGetProperty("name", out var n)) name = n.GetString();
                    if (t.TryGetProperty("description", out var d)) description = d.GetString();
                }
                else if (t.ValueKind == JsonValueKind.String)
                {
                    name = t.GetString();
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    tools.Add(new Tool
                    {
                        Name = name!,
                        Description = description ?? string.Empty,
                        Inputs = new JsonSchema { Type = "object" },
                        Outputs = new JsonSchema { Type = "object" },
                        Tags = Array.Empty<string>(),
                        ToolCallTemplate = new Http.HttpCallTemplate
                        {
                            CallTemplateType = "http",
                            Name = manualName,
                            Method = "GET",
                            Url = new Uri("http://localhost/", UriKind.Absolute),
                        },
                    });
                }
            }
        }

        return new UtcpManual { Tools = tools };
    }

    private static object? TryDeserializeToClosest(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        try
        {
            if ((trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                (trimmed.StartsWith("[") && trimmed.EndsWith("]")) ||
                (trimmed.StartsWith("\"") && trimmed.EndsWith("\"")))
            {
                return JsonSerializer.Deserialize<object?>(trimmed);
            }
        }
        catch
        {
            // fall back to string
        }
        return trimmed;
    }

    private static object? ProcessToolResult(object? parsed)
    {
        if (parsed is null)
        {
            return null;
        }

        if (parsed is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Object)
            {
                if (el.TryGetProperty("structured_output", out var so))
                {
                    return TryDeserializeToClosest(so.GetRawText());
                }

                if (el.TryGetProperty("content", out var contentEl))
                {
                    // Content can be list or object with text/json
                    if (contentEl.ValueKind == JsonValueKind.Array)
                    {
                        var results = new List<object?>();
                        foreach (var item in contentEl.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                if (item.TryGetProperty("text", out var textEl))
                                {
                                    var textValue = textEl.GetString() ?? string.Empty;
                                    results.Add(ParseTextContent(textValue));
                                }
                                else if (item.TryGetProperty("json", out var jsonEl))
                                {
                                    results.Add(TryDeserializeToClosest(jsonEl.GetRawText()));
                                }
                                else
                                {
                                    results.Add(TryDeserializeToClosest(item.GetRawText()));
                                }
                            }
                            else if (item.ValueKind == JsonValueKind.String)
                            {
                                results.Add(ParseTextContent(item.GetString() ?? string.Empty));
                            }
                            else
                            {
                                results.Add(TryDeserializeToClosest(item.GetRawText()));
                            }
                        }
                        if (results.Count == 1)
                        {
                            return results[0];
                        }
                        return results;
                    }
                    else if (contentEl.ValueKind == JsonValueKind.Object)
                    {
                        if (contentEl.TryGetProperty("text", out var textEl))
                        {
                            return ParseTextContent(textEl.GetString() ?? string.Empty);
                        }
                        if (contentEl.TryGetProperty("json", out var jsonEl))
                        {
                            return TryDeserializeToClosest(jsonEl.GetRawText());
                        }
                    }
                }

                if (el.TryGetProperty("result", out var resultEl))
                {
                    return TryDeserializeToClosest(resultEl.GetRawText());
                }

                // Fallback to raw object
                return parsed;
            }

            if (el.ValueKind == JsonValueKind.String)
            {
                return ParseTextContent(el.GetString() ?? string.Empty);
            }

            return parsed;
        }

        if (parsed is string s)
        {
            return ParseTextContent(s);
        }

        return parsed;
    }

    private static object? ParseTextContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }
        var trimmed = text.Trim();
        // Try JSON first
        var jsonCandidate = (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                            (trimmed.StartsWith("[") && trimmed.EndsWith("]")) ||
                            (trimmed.StartsWith("\"") && trimmed.EndsWith("\""));
        if (jsonCandidate)
        {
            try { return JsonSerializer.Deserialize<object?>(trimmed); } catch { }
        }
        // Try numeric
        if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
        {
            return l;
        }
        if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d))
        {
            return d;
        }
        return text;
    }

    private sealed record McpCallRequest
    {
        public required string ToolName { get; init; }
        public required IReadOnlyDictionary<string, object?> Arguments { get; init; }
    }

    private sealed record JsonRpcRequest
    {
        public required string Jsonrpc { get; init; }
        public required string Id { get; init; }
        public required string Method { get; init; }
        public required Dictionary<string, object?> Params { get; init; }
    }

    private async Task<string> GetOAuth2TokenAsync(OAuth2Auth auth, CancellationToken cancellationToken)
    {
        var clientId = auth.ClientId ?? throw new ArgumentException("OAuth2Auth.ClientId is required");
        var cacheKey = $"{auth.TokenUrl}|{clientId}";
        if (this.oauthTokens.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var scopeValue = auth.Scopes is { Length: > 0 } ? string.Join(' ', auth.Scopes) : string.Empty;
        // Try credentials in body
        try
        {
            var bodyPairs = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
            };
            if (!string.IsNullOrEmpty(clientId)) bodyPairs.Add(new("client_id", clientId));
            if (!string.IsNullOrEmpty(auth.ClientSecret)) bodyPairs.Add(new("client_secret", auth.ClientSecret!));
            if (!string.IsNullOrEmpty(scopeValue)) bodyPairs.Add(new("scope", scopeValue));

            using var body = new FormUrlEncodedContent(bodyPairs);
            using var resp = await this.httpClient.PostAsync(auth.TokenUrl, body, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var token = ExtractAccessToken(json);
            this.oauthTokens[cacheKey] = token;
            return token;
        }
        catch (HttpRequestException)
        {
            // fallback to basic auth header method
        }

        // Fallback: Basic auth header
        var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{auth.ClientSecret}"));
        using (var req = new HttpRequestMessage(HttpMethod.Post, auth.TokenUrl))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            var pairs = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
            };
            if (!string.IsNullOrEmpty(scopeValue)) pairs.Add(new("scope", scopeValue));
            req.Content = new FormUrlEncodedContent(pairs);
            using var resp = await this.httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var token = ExtractAccessToken(json);
            this.oauthTokens[cacheKey] = token;
            return token;
        }
    }

    private static string ExtractAccessToken(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("access_token", out var tok) && tok.ValueKind == JsonValueKind.String)
        {
            return tok.GetString()!;
        }
        throw new InvalidOperationException("OAuth2 token response missing access_token");
    }
}


