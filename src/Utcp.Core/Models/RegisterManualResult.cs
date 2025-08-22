// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

public sealed record RegisterManualResult
{
    public required CallTemplate ManualCallTemplate { get; init; }
    public required UtcpManual Manual { get; init; }
    public bool Success { get; init; } = true;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

