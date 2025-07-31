# C# UTCP Implementation

This is a C# implementation of the Universal Tool Calling Protocol (UTCP). It provides a flexible and extensible framework for defining and interacting with tools across a wide variety of communication protocols.

## Introduction

The Universal Tool Calling Protocol (UTCP) is a modern, flexible, and scalable standard for defining and interacting with tools across a wide variety of communication protocols. It is designed to be easy to use, interoperable, and extensible, making it a powerful choice for building and consuming tool-based services.

In contrast to other protocols like MCP, UTCP places a strong emphasis on:

*   **Scalability**: UTCP is designed to handle a large number of tools and providers without compromising performance.
*   **Interoperability**: With support for a wide range of provider types (including HTTP, WebSockets, gRPC, and even CLI tools), UTCP can integrate with almost any existing service or infrastructure.
*   **Ease of Use**: The protocol is built on simple, well-defined C# classes, making it easy for developers to implement and use.

## Getting Started

To get started with the C# UTCP library, you'll need to have the .NET SDK installed.

### Installation

You can add the library to your project using the .NET CLI:

```bash
dotnet add package utcp
```

### Basic Usage

Here's a simple example of how to use the `UtcpClient` to load a tool manual and execute a tool:

```csharp
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using utcp;

var client = new UtcpClient();
client.LoadManual("path/to/your/manual.json");

var inputs = new JsonObject { ["location"] = "London" };
var result = await client.ExecuteToolAsync("get_weather", inputs);

Console.WriteLine(result);
```

## Protocol Specification

UTCP is defined by a set of core data models that describe tools, how to connect to them (providers), and how to secure them (authentication).

### Tool Discovery

For a client to use a tool, it must be provided with a `UtcpManual` object. This manual contains a list of all the tools available from a provider.

#### `UtcpManual` Model

```csharp
public class UTCPManual
{
    public string UtcpVersion { get; set; }
    public string ManualVersion { get; set; }
    public List<Tool> Tools { get; set; }
}
```

### Tool Definition

Each tool is defined by the `Tool` model.

#### `Tool` Model

```csharp
public class Tool
{
    public string Name { get; set; }
    public string Description { get; set; }
    public JsonObject Inputs { get; set; }
    public JsonObject Outputs { get; set; }
    public List<string> Tags { get; set; }
    public int? AverageResponseSize { get; set; }
    public Transport ToolTransport { get; set; }
}
```

### Authentication

UTCP supports several authentication methods to secure tool access. The `auth` object within a transport's configuration specifies the authentication method to use.

#### API Key

Authentication using a static API key, typically sent in a request header.

```json
{
  "auth_type": "api_key",
  "api_key": "$YOUR_SECRET_API_KEY",
  "var_name": "X-API-Key",
  "location": "header"
}
```

#### Basic Auth

Authentication using a username and password.

```json
{
  "auth_type": "basic",
  "username": "your_username",
  "password": "your_password"
}
```

#### OAuth2

Authentication using the OAuth2 client credentials flow.

```json
{
  "auth_type": "oauth2",
  "token_url": "https://auth.example.com/token",
  "client_id": "your_client_id",
  "client_secret": "your_client_secret",
  "scope": "read write"
}
```

### Transports

Transports are at the heart of UTCP's flexibility. They define the communication protocol for a given tool. UTCP supports a wide range of transport types:

*   `http`
*   `cli`
*   `text`
*   `sse`
*   `graphql`
*   `websocket`
*   `grpc`
*   `tcp`
*   `udp`

## Transport Configuration Examples

### HTTP Transport

```json
{
  "transport_type": "http",
  "url": "https://api.example.com/weather",
  "http_method": "GET",
  "auth": {
    "auth_type": "api_key",
    "api_key": "$WEATHER_API_KEY",
    "var_name": "X-Api-Key"
  }
}
```

### CLI Transport

```json
{
  "transport_type": "cli",
  "command": "my-cli-tool",
  "args": ["--input", "{inputValue}"]
}
```

## UTCP Client Architecture

The C# UTCP client provides a robust and extensible framework for interacting with tool providers. Its architecture is designed around a few key components that work together to manage, execute, and search for tools.

### Core Components

*   **`UtcpClient`**: The main entry point for interacting with the UTCP ecosystem.
*   **`UtcpClientConfig`**: A class that defines the client's configuration, including variables for authentication.
*   **`Transport`**: An abstract base class that defines the contract for all transport implementations.
*   **`IToolRepository`**: An interface that defines the contract for storing and retrieving tools.
*   **`IToolSearchStrategy`**: An interface for implementing different tool search algorithms.

### Initialization and Configuration

```csharp
var config = new UtcpClientConfig
{
    Variables = new Dictionary<string, string>
    {
        { "WEATHER_API_KEY", "your-secret-key" }
    }
};

var client = new UtcpClient(config: config);
