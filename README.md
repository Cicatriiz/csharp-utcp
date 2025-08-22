## Universal Tool Calling Protocol (UTCP) 1.0.1 for .NET

Idiomatic .NET SDK for the Universal Tool Calling Protocol (UTCP), mirroring the Python reference implementation.

### What is UTCP?
UTCP (Universal Tool Calling Protocol) is an open, language-agnostic specification for discovering and invoking tools (functions, APIs, and services) in a consistent way across transports such as HTTP, MCP, CLI, text files, and sockets. 

In short, UTCP provides a standardized way for AI systems and other clients to discover and call tools from different providers, regardless of the underlying protocol used (HTTP, WebSocket, CLI, etc.). This specification defines:

- Tool discovery mechanisms
- Tool call formats
- Provider configuration
- Authentication methods
- Response handling

In practice, UTCP lets you register tools from many sources, search them, call them synchronously or as streams, and compose them reliably from any runtime that implements the spec.

See the official [UTCP specification](https://github.com/universal-tool-calling-protocol/utcp-specification) for full details. 

### Features
- Core models and client with async APIs and CancellationToken
- Pluggable communication protocols: HTTP (JSON + SSE), CLI, Text, MCP; Socket and GraphQL scaffolds
- OpenAPI converter to UTCP tools
- In-memory concurrent tool repository and search strategy
- Variable substitution with namespacing and env support
- Post-processing (FilterDict)
- System.Text.Json with source-gen and polymorphism
- Logging via Microsoft.Extensions.Logging

### Repository layout
- `src/Utcp.Core` – core types, client, search, substitution, post-processing, registry
- `src/Utcp.Http` – HTTP protocol (including SSE) and OpenAPI converter
- `src/Utcp.Cli` – CLI protocol (process runner)
- `src/Utcp.Text` – Text protocol (file read and streaming)
- `src/Utcp.Mcp` – MCP protocol (HTTP/stdio, OAuth2, SSE, result shaping)
- `src/Utcp.Socket` – TCP/UDP scaffolds
- `src/Utcp.Gql` – GraphQL scaffold
- `tests/*` – xUnit tests with FluentAssertions
- `samples/*` – client and server samples

### Install / Build
```bash
dotnet build
dotnet test
```

### Quick start
Register a manual and call a tool:
```csharp
using Utcp.Core;
using Utcp.Core.Models;
using Utcp.Http;

var client = await UtcpClientImplementation.CreateAsync(config: new UtcpClientConfig
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
        }
    }
});

var result = await client.CallToolAsync("openlibrary", new Dictionary<string, object?>());
Console.WriteLine(result);
```

### MCP (Model Context Protocol)
Multi-server configuration with OAuth2 and HTTP SSE streaming:
```csharp
using Utcp.Mcp;
using Utcp.Core.Models;

var mcpTemplate = new McpCallTemplate
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
};

var register = await client.RegisterManualAsync(mcpTemplate);
var mcpResult = await client.CallToolAsync("echo", new Dictionary<string, object?>{ ["message"] = "hi" });
await foreach (var chunk in client.CallToolStreamingAsync("echo", new Dictionary<string, object?>{ ["message"] = "hi" }))
{
    Console.WriteLine(chunk);
}
```

### Variable substitution
Namespaced variables follow the pattern `manual__{name}_VAR`. You can pass variables directly or rely on environment variables.
```csharp
using Utcp.Core.Substitution;

var substitutor = new DefaultVariableSubstitutor();
var cfg = new UtcpClientConfig { ToolRepository = repo, ToolSearchStrategy = search, Variables = new(){ ["manual__openlibrary_API_KEY"] = "123" } };
var substituted = substitutor.Substitute(new Dictionary<string, object?> { ["auth"] = new Dictionary<string, object?>{ ["token"] = "${API_KEY}" } }, cfg, "manual_openlibrary");
```

### OpenAPI conversion
End-to-end conversion using `OpenApiToUtcpConverter`:
```csharp
using Utcp.Http.OpenApi;
using System.Net.Http;

// Load an OpenAPI document
var http = new HttpClient();
var specUrl = "https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v3.0/petstore.yaml";
var yaml = await http.GetStringAsync(specUrl);

// Convert (reader supports JSON/YAML via Microsoft.OpenApi.Readers)
var converter = new OpenApiToUtcpConverter();
var manual = converter.ConvertFromString(yaml, specUrl);

// Register the converted tools
var cfg = new UtcpClientConfig
{
    ToolRepository = new Utcp.Core.Repositories.InMemToolRepository(),
    ToolSearchStrategy = new Utcp.Core.Search.TagAndDescriptionWordMatchStrategy(),
    ManualCallTemplates = Array.Empty<CallTemplate>()
};
var client = await UtcpClientImplementation.CreateAsync(config: cfg);
await client.RegisterManualAsync(new HttpCallTemplate { CallTemplateType = "http", Name = manual.Tools.First().ToolCallTemplate.Name, Url = new Uri("http://example") });

// Call one of the converted tools (operationId)
var tools = await client.SearchToolsAsync("pet store", 5);
var anyTool = tools.First();
var output = await client.CallToolAsync(anyTool.Name, new Dictionary<string, object?>());
Console.WriteLine(output);
```
See also `tests/Utcp.Http.Tests/OpenApiConverterTests.cs` for focused examples.

### Samples
- `samples/ClientSample` – demonstrates setup, HTTP + MCP configuration
- `samples/ServerSample` – minimal ASP.NET API with UTCP discovery endpoint

### Full examples with expected outputs

#### CLI
Run a local command and capture stdout/stderr:
```csharp
using Utcp.Cli;

var client = await UtcpClientImplementation.CreateAsync(config: new UtcpClientConfig
{
    ToolRepository = new Utcp.Core.Repositories.InMemToolRepository(),
    ToolSearchStrategy = new Utcp.Core.Search.TagAndDescriptionWordMatchStrategy(),
});

// Register a CLI tool (e.g., echo)
var template = new CliCallTemplate { CallTemplateType = "cli", Name = "local", Command = "/bin/echo" };
await client.RegisterManualAsync(template);

// Call it
var resp = await client.CallToolAsync("local", new Dictionary<string, object?> { ["args"] = new [] { "hello" } });
Console.WriteLine(resp); // expected: "hello\n"
```

#### Text
Read a file fully and as a stream of chunks:
```csharp
using Utcp.Text;

var text = new TextCallTemplate { CallTemplateType = "text", Name = "docs", FilePath = "README.md" };
await client.RegisterManualAsync(text);

var all = await client.CallToolAsync("docs", new Dictionary<string, object?>());
Console.WriteLine(((string)all!).Length > 0); // expected: True

var streamed = client.CallToolStreamingAsync("docs", new Dictionary<string, object?>());
await foreach (var chunk in streamed)
{
    Console.WriteLine(chunk); // expected: lines (or chunks if ChunkSizeBytes set)
}
```

#### MCP
Echo via MCP HTTP and stdio with result shaping:
```csharp
using Utcp.Mcp;

var mcp = new McpCallTemplate
{
    CallTemplateType = "mcp",
    Name = "mcp",
    Config = new McpConfig
    {
        McpServers = new Dictionary<string, McpServerConfig>
        {
            ["http"] = new() { Url = "http://localhost:7400/mcp" },
            ["stdio"] = new() { Command = "my-mcp-server" },
        }
    }
};
await client.RegisterManualAsync(mcp);

var r = await client.CallToolAsync("echo", new Dictionary<string, object?> { ["message"] = "test" });
Console.WriteLine(r); // expected: { reply = "you said: test" } or similar shaped object/string

await foreach (var s in client.CallToolStreamingAsync("echo", new Dictionary<string, object?> { ["message"] = "test" }))
{
    Console.WriteLine(s); // expected: streamed chunks/events if server uses SSE
}
```

### Protocol registration
Protocols are registered in `Utcp.Core.Protocols.ProtocolRegistry`. Custom protocols can be added at runtime:
```csharp
using Utcp.Core.Protocols;
ProtocolRegistry.Register("my-protocol", new MyProtocol());
```

### Post-processing
Use post-processors to shape results. A simple filter example:
```csharp
using Utcp.Core.PostProcessing;
var cfg = new UtcpClientConfig { ToolRepository = repo, ToolSearchStrategy = search, PostProcessing = new [] { new FilterDictPostProcessor(new []{"allowed"}) } };
```

### Tool search
Default search `TagAndDescriptionWordMatchStrategy` ranks tools by tag hits and description term overlap:
```csharp
var results = await client.SearchToolsAsync("weather forecast", limit: 5);
```

### Concurrency and thread safety
`InMemToolRepository` uses async synchronization to support concurrent reads/writes safely. All client operations are async and accept `CancellationToken`.

### Error handling
- Rich argument and state validation with specific exceptions
- HTTP paths use `EnsureSuccessStatusCode` and propagate details
- MCP stdio paths surface process start and JSON parsing errors

### Parity with Python
- Data models map 1:1 (Auth, CallTemplate, Tool, UtcpManual, UtcpClientConfig, RegisterManualResult)
- MCP mirrors registration, calling, streaming, OAuth2 token flows, and result shaping
- OpenAPI converter maps security schemes and schemas similarly; see tests for parity
- Variable substitution supports namespacing and env resolution

### CI & Packaging
- GitHub Actions builds/tests/packs on Windows, Linux, macOS (`.github/workflows/build.yml`)
- NuGet metadata via `Directory.Build.props` (packages per project)

### License
This project is licensed under the Mozilla Public License 2.0 (MPL-2.0). See `LICENSE` for details.



