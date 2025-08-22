// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Http.OpenApi;
using FluentAssertions;
using Xunit;

public class OpenApiConverterTests
{
    [Fact]
    public void ConvertsSimpleDoc()
    {
        const string json = """
        {
          "openapi": "3.0.0",
          "info": {"title": "t", "version": "1.0.0"},
          "servers": [{"url": "https://api.example.com"}],
          "paths": {
            "/status": {
              "get": {"operationId": "getStatus", "summary": "status"}
            }
          }
        }
        """;

        var conv = new OpenApiToUtcpConverter();
        var manual = conv.FromJson(json, "manual");

        manual.Tools.Should().ContainSingle(t => t.Name == "manual.getStatus");
    }
}

