// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Socket;

using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record TcpCallTemplate : CallTemplate
{
    public required string Host { get; init; }
    public int Port { get; init; }

    public TcpCallTemplate()
    {
        CallTemplateType = "tcp";
        PolymorphicRegistry.RegisterCallTemplateDerivedType("tcp", typeof(TcpCallTemplate));
    }
}


