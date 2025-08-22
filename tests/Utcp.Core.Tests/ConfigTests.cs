// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;
using Utcp.Core;
using Utcp.Core.Models;
using Utcp.Core.Substitution;
using FluentAssertions;
using Xunit;

public class ConfigTests
{
    [Fact]
    public void VariableSubstitution_Namespace()
    {
        var substitutor = new DefaultVariableSubstitutor();
        var cfg = new UtcpClientConfig
        {
            Variables = new Dictionary<string, string>
            {
                { "manual__openlibrary_API_KEY", "123" },
            },
            ToolRepository = null!,
            ToolSearchStrategy = null!,
        };

        var input = new Dictionary<string, object?>
        {
            { "auth", new Dictionary<string, object?> { { "token", "${API_KEY}" } } },
        };

        var result = (Dictionary<string, object?>)substitutor.Substitute(input, cfg, "manual_openlibrary")!;
        ((Dictionary<string, object?>)result["auth"])!["token"].Should().Be("123");
    }
}

