// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Text;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record TextCallTemplate : CallTemplate
{
    static TextCallTemplate()
    {
        PolymorphicRegistry.RegisterCallTemplateDerivedType("text", typeof(TextCallTemplate));
    }

    public required string FilePath { get; init; }
    public string? EncodingName { get; init; }
    public int ChunkSizeBytes { get; init; } = 0;
    public bool EnsureUnderRoot { get; init; } = true;

    public TextCallTemplate()
    {
        CallTemplateType = "text";
    }
}

