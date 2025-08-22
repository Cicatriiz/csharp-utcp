// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

public sealed record UtcpManual
{
    public string UtcpVersion { get; init; } = "1.0.0";
    public string ManualVersion { get; init; } = "1.0.0";
    public required IReadOnlyList<Tool> Tools { get; init; }
}

