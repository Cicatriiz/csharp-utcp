// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Interfaces;

using Utcp.Core.Models;

public interface IConcurrentToolRepository
{
    Task SaveManualAsync(CallTemplate manualCallTemplate, UtcpManual manual, CancellationToken cancellationToken = default);
    Task<bool> RemoveManualAsync(string manualName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetManualNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UtcpManual>> GetManualsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tool>> GetToolsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CallTemplate>> GetManualCallTemplatesAsync(CancellationToken cancellationToken = default);

    Task<UtcpManual?> TryGetManualByNameAsync(string manualName, CancellationToken cancellationToken = default);

    Task<CallTemplate?> TryGetManualCallTemplateByNameAsync(string manualName, CancellationToken cancellationToken = default);
}

