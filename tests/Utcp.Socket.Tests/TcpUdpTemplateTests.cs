// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Socket;
using FluentAssertions;
using Xunit;

public class TcpUdpTemplateTests
{
    [Fact]
    public void TcpTemplate_StoresHostPort()
    {
        var t = new TcpCallTemplate { CallTemplateType = "tcp", Name = "manual", Host = "localhost", Port = 1234 };
        t.Host.Should().Be("localhost");
        t.Port.Should().Be(1234);
    }

    [Fact]
    public void UdpTemplate_StoresHostPort()
    {
        var t = new UdpCallTemplate { CallTemplateType = "udp", Name = "manual", Host = "localhost", Port = 9876 };
        t.Host.Should().Be("localhost");
        t.Port.Should().Be(9876);
    }
}

