// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using System.Text.Json.Serialization;
using Utcp.Core.Models;

[JsonSerializable(typeof(UtcpClientConfig))]
[JsonSerializable(typeof(UtcpManual))]
[JsonSerializable(typeof(Tool))]
public partial class UtcpJsonContext : JsonSerializerContext
{
}

