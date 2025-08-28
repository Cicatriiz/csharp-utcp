// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http;

using System.Net.Http.Json;
using Utcp.Core;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;
using Utcp.Http.OpenApi;

public sealed class HttpCommunicationProtocol : ICommunicationProtocol
{
    private readonly IHttpClientFactory httpClientFactory;

    public HttpCommunicationProtocol(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<RegisterManualResult> RegisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        // Auto-convert OpenAPI specs into UTCP manuals if the manual template points to an OpenAPI document
        if (manualCallTemplate is HttpCallTemplate http && http.Url is not null)
        {
            var client = this.httpClientFactory.CreateClient("utcp");
            if (http.Timeout is not null)
            {
                client.Timeout = http.Timeout.Value;
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, http.Url);
            if (http.Headers is not null)
            {
                foreach (var (k, v) in http.Headers)
                {
                    req.Headers.TryAddWithoutValidation(k, v);
                }
            }

            try
            {
                using var resp = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
                resp.EnsureSuccessStatusCode();
                var specText = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                // Heuristic: treat JSON OpenAPI documents (we look for root object text)
                var trimmed = specText.TrimStart();
                if (trimmed.StartsWith("{"))
                {
                    var converter = new OpenApiToUtcpConverter();
                    var manual = converter.FromJson(specText, http.Name ?? string.Empty);
                    return new RegisterManualResult
                    {
                        ManualCallTemplate = manualCallTemplate,
                        Manual = manual,
                        Success = true,
                    };
                }
            }
            catch (Exception ex)
            {
                return new RegisterManualResult
                {
                    ManualCallTemplate = manualCallTemplate,
                    Manual = new UtcpManual { Tools = Array.Empty<Tool>() },
                    Success = false,
                    Errors = new[] { ex.Message },
                };
            }

            // Not an OpenAPI JSON document; default to no tools
            return new RegisterManualResult
            {
                ManualCallTemplate = manualCallTemplate,
                Manual = new UtcpManual { Tools = Array.Empty<Tool>() },
                Success = true,
            };
        }

        // For unsupported template types, return empty manual by default
        return new RegisterManualResult
        {
            ManualCallTemplate = manualCallTemplate,
            Manual = new UtcpManual { Tools = Array.Empty<Tool>() },
            Success = true,
        };
    }

    public Task DeregisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<object?> CallToolAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is not HttpCallTemplate http)
        {
            throw new ArgumentException("Expected HttpCallTemplate");
        }

        var client = this.httpClientFactory.CreateClient("utcp");
        if (http.Timeout is not null)
        {
            client.Timeout = http.Timeout.Value;
        }

        using var req = new HttpRequestMessage(new HttpMethod(http.Method), http.Url);
        if (http.Headers is not null)
        {
            foreach (var (k, v) in http.Headers)
            {
                req.Headers.TryAddWithoutValidation(k, v);
            }
        }

        if (http.Body is not null)
        {
            req.Content = JsonContent.Create(http.Body);
        }

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var text = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return text;
    }

    public async IAsyncEnumerable<object?> CallToolStreamingAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (toolCallTemplate is not StreamableHttpCallTemplate http)
        {
            // Fallback to single-shot
            var result = await this.CallToolAsync(caller, toolName, toolArgs, toolCallTemplate, cancellationToken).ConfigureAwait(false);
            yield return result;
            yield break;
        }

        var client = this.httpClientFactory.CreateClient("utcp");
        if (http.Timeout is not null)
        {
            client.Timeout = Timeout.InfiniteTimeSpan; // streaming
        }

        using var req = new HttpRequestMessage(new HttpMethod(http.Method), http.Url);
        req.Headers.Accept.ParseAdd(http.ContentType);
        if (http.Headers is not null)
        {
            foreach (var (k, v) in http.Headers)
            {
                req.Headers.TryAddWithoutValidation(k, v);
            }
        }
        if (http.Body is not null)
        {
            req.Content = JsonContent.Create(http.Body);
        }

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        await foreach (var evt in SseReader.ReadAsync(resp, cancellationToken).ConfigureAwait(false))
        {
            yield return evt.Data;
        }
    }
}

