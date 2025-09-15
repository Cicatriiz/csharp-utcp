// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using FluentAssertions;
using Utcp.Core.Models;
using Utcp.Core.Repositories;
using Utcp.Http;

public sealed class InMemToolRepositoryTests
{
    [Fact]
    public async Task RemoveToolAsync_RemovesFromLookupAndManual()
    {
        var repository = new InMemToolRepository();
        var manualTemplate = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("https://api.example.com/utcp"),
        };

        var tool = new Tool
        {
            Name = "manual.echo",
            ToolCallTemplate = manualTemplate,
        };

        var manual = new UtcpManual
        {
            Tools = new[] { tool },
        };

        await repository.SaveManualAsync(manualTemplate, manual);

        (await repository.GetToolAsync("manual.echo")).Should().NotBeNull();

        var removed = await repository.RemoveToolAsync("manual.echo");
        removed.Should().BeTrue();
        (await repository.GetToolAsync("manual.echo")).Should().BeNull();

        var storedManual = await repository.GetManualAsync("manual");
        storedManual.Should().NotBeNull();
        storedManual!.Tools.Should().BeEmpty();
    }

    [Fact]
    public async Task GetManualCallTemplateAsync_ReturnsStoredTemplate()
    {
        var repository = new InMemToolRepository();
        var manualTemplate = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "manual",
            Url = new Uri("https://api.example.com/utcp"),
        };

        var manual = new UtcpManual
        {
            Tools = Array.Empty<Tool>(),
        };

        await repository.SaveManualAsync(manualTemplate, manual);

        var retrieved = await repository.GetManualCallTemplateAsync("manual");
        retrieved.Should().BeOfType<HttpCallTemplate>().Which.Should().BeEquivalentTo(manualTemplate);
    }

    [Fact]
    public async Task GetToolsByManualAsync_ReturnsNullForUnknownManual()
    {
        var repository = new InMemToolRepository();
        var result = await repository.GetToolsByManualAsync("unknown");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveToolAsync_ReturnsFalseWhenMissing()
    {
        var repository = new InMemToolRepository();
        var removed = await repository.RemoveToolAsync("missing");
        removed.Should().BeFalse();
    }
}
