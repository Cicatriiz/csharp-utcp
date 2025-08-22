// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http;

using System.Net.Http.Json;
using Utcp.Core;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class HttpCommunicationProtocol : ICommunicationProtocol
{
    private readonly IHttpClientFactory httpClientFactory;

    public HttpCommunicationProtocol(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public Task<RegisterManualResult> RegisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

