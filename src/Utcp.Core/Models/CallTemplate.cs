// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

public abstract record CallTemplate
{
    public required string CallTemplateType { get; init; }
    public required string Name { get; init; }
    public Auth? Auth { get; init; }
}

