// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Cli;
using FluentAssertions;
using Xunit;
using System.Runtime.InteropServices;

public class ProcessRunnerTests
{
    [Fact]
    public async Task EchoCommand_Works()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var template = new CliCallTemplate
        {
            CallTemplateType = "cli",
            Name = "manual",
            Command = isWindows ? "cmd" : "/bin/echo",
            Args = isWindows ? new []{"/c", "echo", "hello"} : new []{"hello"},
        };

        var (code, stdOut, stdErr) = await ProcessRunner.RunAsync(template, default);
        code.Should().Be(0);
        stdOut.Trim().Should().Be("hello");
        stdErr.Should().BeEmpty();
    }
}

