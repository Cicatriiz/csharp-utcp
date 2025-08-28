// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Core.Models;
using Utcp.Core.Repositories;
using Utcp.Core.Search;
using FluentAssertions;
using Xunit;

public class SearchStrategyTests
{
    [Fact]
    public async Task MatchesByTagsAndDescription()
    {
        var repo = new InMemToolRepository();
        var strategy = new TagAndDescriptionWordMatchStrategy
        {
            DescriptionWeight = 1,
            TagWeight = 3,
        };

        var manual = new UtcpManual
        {
            Tools = new List<Tool>
            {
                new Tool
                {
                    Name = "m.weather.current",
                    Description = "Get current weather by city",
                    Tags = new []{"weather", "forecast"},
                    ToolCallTemplate = new DummyCallTemplate { CallTemplateType = "dummy", Name = "dummy" },
                },
                new Tool
                {
                    Name = "m.math.add",
                    Description = "Add two numbers",
                    Tags = new []{"math", "calculator"},
                    ToolCallTemplate = new DummyCallTemplate { CallTemplateType = "dummy", Name = "dummy" },
                },
            }
        };
        await repo.SaveManualAsync(new DummyCallTemplate{ CallTemplateType = "dummy", Name = "m" }, manual);

        var results = await strategy.SearchToolsAsync(repo, "weather in city", 10);
        results.Should().NotBeEmpty();
        results.First().Name.Should().Be("m.weather.current");
    }

    [Fact]
    public async Task FiltersByAnyOfTagsRequired()
    {
        var repo = new InMemToolRepository();
        var strategy = new TagAndDescriptionWordMatchStrategy();

        var manual = new UtcpManual
        {
            Tools = new List<Tool>
            {
                new Tool
                {
                    Name = "t1",
                    Description = "alpha",
                    Tags = new[]{"x","y"},
                    ToolCallTemplate = new DummyCallTemplate { CallTemplateType = "dummy", Name = "dummy" },
                },
                new Tool
                {
                    Name = "t2",
                    Description = "beta",
                    Tags = new[]{"z"},
                    ToolCallTemplate = new DummyCallTemplate { CallTemplateType = "dummy", Name = "dummy" },
                }
            }
        };
        await repo.SaveManualAsync(new DummyCallTemplate{ CallTemplateType = "dummy", Name = "m" }, manual);

        var results = await strategy.SearchToolsAsync(repo, "", 10, new[]{"z"});
        results.Should().ContainSingle();
        results.Single().Name.Should().Be("t2");
    }

    private sealed record DummyCallTemplate : CallTemplate;
}

