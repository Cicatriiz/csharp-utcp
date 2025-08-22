// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Core.Models;
using Utcp.Core.Repositories;
using FluentAssertions;
using Xunit;

public class RepositoryConcurrencyTests
{
    [Fact]
    public async Task SaveManual_IsThreadSafe()
    {
        var repo = new InMemToolRepository();

        var manual = new UtcpManual
        {
            Tools = new List<Tool>
            {
                new Tool
                {
                    Name = "m.calc.add",
                    ToolCallTemplate = new DummyCallTemplate { CallTemplateType = "dummy", Name = "dummy" },
                },
            },
        };
        var template = new DummyCallTemplate { CallTemplateType = "dummy", Name = "m" };

        var tasks = Enumerable.Range(0, 25).Select(_ => repo.SaveManualAsync(template, manual));
        await Task.WhenAll(tasks);

        var tools = await repo.GetToolsAsync();
        tools.Should().ContainSingle(t => t.Name == "m.calc.add");
    }

    private sealed record DummyCallTemplate : CallTemplate;
}

