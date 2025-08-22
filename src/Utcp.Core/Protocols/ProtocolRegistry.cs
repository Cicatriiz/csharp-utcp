// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Protocols;

using Utcp.Core.Interfaces;

public static class ProtocolRegistry
{
    private static readonly Dictionary<string, ICommunicationProtocol> Protocols = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string type, ICommunicationProtocol protocol, bool overrideExisting = false)
    {
        if (!overrideExisting && Protocols.ContainsKey(type))
        {
            return;
        }

        Protocols[type] = protocol;
    }

    public static bool TryGet(string type, out ICommunicationProtocol? protocol)
    {
        return Protocols.TryGetValue(type, out protocol);
    }
}

