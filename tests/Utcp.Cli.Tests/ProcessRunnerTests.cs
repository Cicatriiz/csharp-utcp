// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Cli;
using FluentAssertions;
using Xunit;

public class ProcessRunnerTests
{
    [Fact]
    public async Task EchoCommand_Works()
    {
        var template = new CliCallTemplate
        {
            CallTemplateType = "cli",
            Name = "manual",
            Command = "/bin/echo",
            Args = new []{"hello"},
        };

        var (code, stdOut, stdErr) = await ProcessRunner.RunAsync(template, default);
        code.Should().Be(0);
        stdOut.Trim().Should().Be("hello");
        stdErr.Should().BeEmpty();
    }
}

