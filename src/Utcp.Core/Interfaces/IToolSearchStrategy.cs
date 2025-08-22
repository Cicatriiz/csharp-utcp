// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Interfaces;

using Utcp.Core.Models;

public interface IToolSearchStrategy
{
    Task<IReadOnlyList<Tool>> SearchToolsAsync(IConcurrentToolRepository toolRepository, string query, int limit = 10, IReadOnlyList<string>? anyOfTagsRequired = null, CancellationToken cancellationToken = default);
}

