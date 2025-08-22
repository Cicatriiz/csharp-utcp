// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Utcp.Core;
using Utcp.Core.Models;
using Utcp.Http;
using Utcp.Http.OpenApi;
using Utcp.Mcp;

var config = new UtcpClientConfig
{
    ToolRepository = new Utcp.Core.Repositories.InMemToolRepository(),
    ToolSearchStrategy = new Utcp.Core.Search.TagAndDescriptionWordMatchStrategy(),
    ManualCallTemplates = new []
    {
        (CallTemplate)new HttpCallTemplate
        {
            CallTemplateType = "http",
            Name = "openlibrary",
            Method = "GET",
            Url = new Uri("https://openlibrary.org/works/OL45883W.json"),
        },
        new McpCallTemplate
        {
            CallTemplateType = "mcp",
            Name = "mcp",
            Config = new McpConfig
            {
                McpServers = new Dictionary<string, McpServerConfig>
                {
                    ["stdioServer"] = new() { Command = "my-mcp-server" },
                    ["httpServer"] = new() { Url = "http://localhost:7400/mcp", Headers = new Dictionary<string,string>{{"X-Client","Utcp"}} },
                }
            },
            Auth = new OAuth2Auth { AuthType = "oauth2", TokenUrl = "http://localhost:7400/token", ClientId = "id", ClientSecret = "secret" }
        }
    }
};

var client = await Utcp.Core.UtcpClientImplementation.CreateAsync(config: config);
Console.WriteLine("Sample client initialized.");
