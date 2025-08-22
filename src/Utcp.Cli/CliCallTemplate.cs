// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Cli;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record CliCallTemplate : CallTemplate
{
    public required string Command { get; init; }
    public IReadOnlyList<string>? Args { get; init; }
    public string? WorkingDirectory { get; init; }
    public IReadOnlyDictionary<string, string>? Environment { get; init; }
    public TimeSpan? Timeout { get; init; }

    public CliCallTemplate()
    {
        CallTemplateType = "cli";
        PolymorphicRegistry.RegisterCallTemplateDerivedType("cli", typeof(CliCallTemplate));
    }
}

