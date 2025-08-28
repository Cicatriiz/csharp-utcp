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

public class VariableLoaderTests
{
    [Fact]
    public void ResolutionOrder_Config_Loaders_Env()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var dotenvPath = Path.Combine(tempDir.FullName, ".env");
            File.WriteAllText(dotenvPath, "FOO=from_loader\nBAR=from_loader\n");
            Environment.SetEnvironmentVariable("FOO", "from_env");
            Environment.SetEnvironmentVariable("BAZ", "from_env");

            var cfg = new UtcpClientConfig
            {
                Variables = new Dictionary<string, string>
                {
                    { "FOO", "from_config" },
                },
                LoadVariablesFrom = new [] { (IVariableLoader)new Utcp.Core.Substitution.DotEnvVariableLoader(tempDir.FullName) },
                ToolRepository = null!,
                ToolSearchStrategy = null!,
            };

            var substitutor = new DefaultVariableSubstitutor();
            var input = new Dictionary<string, object?>
            {
                { "a", "${FOO}" }, // config wins
                { "b", "${BAR}" }, // loader wins
                { "c", "${BAZ}" }, // env wins
            };
            var result = (Dictionary<string, object?>)substitutor.Substitute(input, cfg)!;

            result["a"].Should().Be("from_config");
            result["b"].Should().Be("from_loader");
            result["c"].Should().Be("from_env");
            Assert.Throws<Utcp.Core.Interfaces.UtcpVariableNotFound>(() => substitutor.Substitute("${REALLY_MISSING}", cfg));
        }
        finally
        {
            Environment.SetEnvironmentVariable("FOO", null);
            Environment.SetEnvironmentVariable("BAZ", null);
        }
    }
}

