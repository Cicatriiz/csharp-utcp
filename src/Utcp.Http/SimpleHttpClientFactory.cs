// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Http;

using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class SimpleHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler handler;

    public SimpleHttpClientFactory()
    {
        this.handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(this.handler, disposeHandler: false)
        {
            Timeout = TimeSpan.FromSeconds(100),
        };
    }
}

