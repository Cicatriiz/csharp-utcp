// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Gql;
using FluentAssertions;
using Xunit;

public class GraphQlCallTemplateTests
{
    [Fact]
    public void Template_StoresEndpoint()
    {
        var t = new GraphQlCallTemplate { CallTemplateType = "gql", Name = "manual", Endpoint = new Uri("https://example.com/graphql") };
        t.Endpoint.Should().Be(new Uri("https://example.com/graphql"));
    }
}

