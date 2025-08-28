// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Interfaces;

public sealed class UtcpSerializerValidationError : Exception
{
    public UtcpSerializerValidationError(string message) : base(message) {}
}

public sealed class UtcpVariableNotFound : Exception
{
    public UtcpVariableNotFound(string variableName) : base($"Variable not found: {variableName}")
    {
        this.VariableName = variableName;
    }

    public string VariableName { get; }
}


