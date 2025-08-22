// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Models;

public abstract record Auth
{
    public required string AuthType { get; init; }
}

public sealed record ApiKeyAuth : Auth
{
    public required string KeyName { get; init; }
    public string? Location { get; init; }
}

public sealed record BasicAuth : Auth
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed record OAuth2Auth : Auth
{
    public required string TokenUrl { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string[]? Scopes { get; init; }
}

