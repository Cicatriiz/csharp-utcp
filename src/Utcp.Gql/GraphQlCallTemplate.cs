// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Gql;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record GraphQlCallTemplate : CallTemplate
{
    public required Uri Endpoint { get; init; }
    public string? Query { get; init; }

    public GraphQlCallTemplate()
    {
        CallTemplateType = "gql";
        PolymorphicRegistry.RegisterCallTemplateDerivedType("gql", typeof(GraphQlCallTemplate));
    }
}


