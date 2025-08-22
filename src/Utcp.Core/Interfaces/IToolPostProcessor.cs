// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Interfaces;

using Utcp.Core.Models;

public interface IToolPostProcessor
{
    object? PostProcess(UtcpClient caller, Tool tool, CallTemplate manualCallTemplate, object? result);
}

