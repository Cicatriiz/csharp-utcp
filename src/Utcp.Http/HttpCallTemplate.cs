// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http;

using System.Net.Http.Headers;
using Utcp.Core.Models;
using Utcp.Core.Serialization;

public sealed record HttpCallTemplate : CallTemplate
{
    public string Method { get; init; } = "GET";
    public required Uri Url { get; init; }
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
    public object? Body { get; init; }
    public TimeSpan? Timeout { get; init; }

    public HttpCallTemplate()
    {
        CallTemplateType = "http";
        PolymorphicRegistry.RegisterCallTemplateDerivedType("http", typeof(HttpCallTemplate));
    }
}

