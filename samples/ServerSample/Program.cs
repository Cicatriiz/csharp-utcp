// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Core.Models;
using Utcp.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/utcp", () =>
{
    var tool = new Utcp.Core.Models.Tool
    {
        Name = "server.hello",
        Description = "Say hello",
        Inputs = new JsonSchema { Type = "object" },
        Outputs = new JsonSchema { Type = "object" },
        Tags = new []{"sample"},
        ToolCallTemplate = new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "server",
            Method = "GET",
            Url = new Uri("http://localhost:5000/hello"),
        },
    };
    var manual = new UtcpManual { Tools = new []{ tool } };
    return Results.Json(manual);
});

app.MapGet("/hello", () => new { message = "hello" });

app.Run();
