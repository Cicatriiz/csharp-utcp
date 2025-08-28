// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using System.Threading;

public static class PluginLoader
{
    private static int initialized;

    public static void EnsurePluginsInitialized()
    {
        if (Interlocked.Exchange(ref initialized, 1) == 1)
        {
            return;
        }
        // Intentionally left empty in Core. Plugin assemblies should call discovery
        // register methods during their module initialization to wire up built-ins.
    }
}


