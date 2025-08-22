// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Interfaces;

using Utcp.Core.Models;

public interface ICommunicationProtocol
{
    Task<RegisterManualResult> RegisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default);

    Task DeregisterManualAsync(UtcpClient caller, CallTemplate manualCallTemplate, CancellationToken cancellationToken = default);

    Task<object?> CallToolAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, CancellationToken cancellationToken = default);

    IAsyncEnumerable<object?> CallToolStreamingAsync(UtcpClient caller, string toolName, IReadOnlyDictionary<string, object?> toolArgs, CallTemplate toolCallTemplate, CancellationToken cancellationToken = default);
}

