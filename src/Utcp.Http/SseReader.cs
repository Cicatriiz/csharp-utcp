// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http;

using System.Net.Http;
using System.Text;

public static class SseReader
{
    public static async IAsyncEnumerable<SseEvent> ReadAsync(HttpResponseMessage response, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);

        string? eventName = null;
        var dataBuilder = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            if (line.Length == 0)
            {
                if (dataBuilder.Length > 0)
                {
                    yield return new SseEvent(eventName, dataBuilder.ToString());
                    eventName = null;
                    dataBuilder.Clear();
                }
                continue;
            }

            if (line.StartsWith(":"))
            {
                continue; // comment
            }

            var idx = line.IndexOf(':');
            if (idx < 0)
            {
                continue;
            }

            var field = line.Substring(0, idx).Trim();
            var value = line.Substring(idx + 1).TrimStart();

            if (string.Equals(field, "event", StringComparison.OrdinalIgnoreCase))
            {
                eventName = value;
            }
            else if (string.Equals(field, "data", StringComparison.OrdinalIgnoreCase))
            {
                if (dataBuilder.Length > 0)
                {
                    dataBuilder.Append('\n');
                }
                dataBuilder.Append(value);
            }
        }

        if (dataBuilder.Length > 0)
        {
            yield return new SseEvent(eventName, dataBuilder.ToString());
        }
    }
}

