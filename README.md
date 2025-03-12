[![Build](https://github.com/afrise/MCPSharp/actions/workflows/build.yml/badge.svg)](https://github.com/afrise/MCPSharp/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/MCPSharp)](https://www.nuget.org/packages/MCPSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MCPSharp)](https://www.nuget.org/packages/MCPSharp)

# MCPSharp

MCPSharp is a .NET library that helps you build Model Context Protocol (MCP) servers and clients - the standardized API protocol used by AI assistants and models. With MCPSharp, you can:

- Create MCP-compliant tools and functions that AI models can discover and use
- Connect directly to existing MCP servers from C# code with an easy to use client
- Expose your .NET methods as MCP endpoints with simple attributes
- Handle MCP protocol details and JSON-RPC communication seamlessly

## 🚀 What's New in MCPSharp

- **Microsoft.Extensions.AI Integration**: MCPSharp now integrates with Microsoft.Extensions.AI, allowing tools to be exposed as AIFunctions
- **Semantic Kernel Support**: Add tools using Semantic Kernel's KernelFunctionAttribute
- **Dynamic Tool Registration**: Register tools on-the-fly with custom implementation logic
- **Tool Change Notifications**: Server now notifies clients when tools are added, updated, or removed
- **Complex Object Parameter Support**: Better handling of complex objects in tool parameters
- **Better Error Handling**: Improved error handling with detailed stack traces

## When to Use MCPSharp

Use MCPSharp when you want to:
- Create tools that AI assistants like Anthropic's Claude Desktop can use
- Build MCP-compliant APIs without dealing with the protocol details
- Expose existing .NET code as MCP endpoints
- Add AI capabilities to your applications through standardized interfaces
- Integrate with Microsoft.Extensions.AI and/or Semantic Kernel without locking into a single vendor

## Features

- Easy-to-use attribute-based API (`[McpTool]`, `[McpResource]`)
- Built-in JSON-RPC support with automatic request/response handling
- Automatic parameter validation and type conversion
- Rich documentation support through XML comments
- Near zero configuration required for basic usage

## Prerequisites

- Any version of .NET that supports [standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0)

## Installation

```bash
dotnet add package MCPSharp
```

## Quick Start

### 1. Define a Tool

Create a class and mark your method(s) with the `[McpTool]` attribute:

```csharp
using MCPSharp;

public class Calculator
{
    [McpTool("add", "Adds two numbers")]  // Note: [McpFunction] is deprecated, use [McpTool] instead
    public static int Add([McpParameter(true)] int a, [McpParameter(true)] int b)
    {
        return a + b;
    }
}
```

### 2. Start the Server

```csharp
await MCPServer.StartAsync("CalculatorServer", "1.0.0");
```

The StartAsync() method will automatically find any methods in the base assembly that are marked with the McpTool attribute. In order to add any methods that are in a referenced library, you can manually register them by calling `MCPServer.Register<T>();` with `T` being the class containing the desired methods. If your methods are marked with Semantic Kernel attributes, this will work as well. If the client supports list changed notifications, it will be notified when additional tools are registered.

## Advanced Usage

### Dynamic Tool Registration

Register tools dynamically with custom implementation:

```csharp
MCPServer.AddToolHandler(new Tool() 
{
    Name = "dynamicTool",
    Description = "A dynamic tool",
    InputSchema = new InputSchema {
        Type = "object",
        Required = ["input"],
        Properties = new Dictionary<string, ParameterSchema>{
            {"input", new ParameterSchema{Type="string", Description="Input value"}}
        }
    }
}, (string input) => { return $"You provided: {input}"; });
```

### Use with Microsoft.Extensions.AI

```csharp
// Client-side integration
MCPClient client = new("AIClient", "1.0", "path/to/mcp/server");
IList<AIFunction> functions = await client.GetFunctionsAsync();
```
This list can be plugged into the [ChatOptions.Tools](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.chatoptions?view=net-9.0-pp) property for an [IChatClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient?view=net-9.0-pp), Allowing MCP servers to be used seamlessly with Any IChatClient Implementation.


### Semantic Kernel Integration

```csharp
using Microsoft.SemanticKernel;

public class MySkillClass
{
    [KernelFunction("MyFunction")]
    [Description("Description of my function")]
    public string MyFunction(string input) => $"Processed: {input}";
}

// Register with MCPServer
MCPServer.Register<MySkillClass>();
```
Currently, This is the only way to make a Semantic kernel method registerable with the MCP server. If you have a use case that is not covered here, please reach out!


## API Reference

### Attributes

- `[McpTool]` - Marks a class or method as an MCP tool
    -  Optional parameters:
        - `Name` - The tool name (default: class/method name)
        - `Description` - Description of the tool

- `[McpParameter]` - Provides metadata for function parameters
    - Optional parameters:
        - `Description` - Parameter description
        - `Required` - Whether the parameter is required (default: false)

- `[McpResource]` - Marks a property or method as an MCP resource
    - Parameters:
        - `Name` - Resource name
        - `Uri` - Resource URI (can include templates)
        - `MimeType` - MIME type of the resource
        - `Description` - Resource description

### Server Methods

- `MCPServer.StartAsync(string serverName, string version)` - Starts the MCP server
- `MCPServer.Register<T>()` - Registers a class containing tools or resources
- `MCPServer.AddToolHandler(Tool tool, Delegate func)` - Registers a dynamic tool

### Client Methods

- `new MCPClient(string name, string version, string server, string args = null, IDictionary<string, string> env = null)` - Create a client instance
- `client.GetToolsAsync()` - Get available tools
- `client.CallToolAsync(string name, Dictionary<string, object> parameters)` - Call a tool
- `client.GetResourcesAsync()` - Get available resources
- `client.GetFunctionsAsync()` - Get tools as AIFunctions

## XML Documentation Support

MCPSharp automatically extracts documentation from XML comments:

```csharp
/// <summary>
/// Provides mathematical operations
/// </summary>
public class Calculator
{
    /// <summary>
    /// Adds two numbers together
    /// </summary>
    /// <param name="a">The first number to add</param>
    /// <param name="b">The second number to add</param>
    /// <returns>The sum of the two numbers</returns>
    [McpTool]
    public static int Add(
        [McpParameter(true)] int a,
        [McpParameter(true)] int b)
    {
        return a + b;
    }
}
```

Enable XML documentation in your project file:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

This allows you to be able to quickly change the names and descriptions of your MCP tools without having to recompile.  For example, if you find the model is having trouble understanding how to use it correctly.

## Migration Notes

- `[McpFunction]` is deprecated and replaced with `[McpTool]` for better alignment with MCP standards
- Use `MCPServer.Register<T>()` instead of `MCPServer.RegisterTool<T>()` for consistency (old method still works but is deprecated)

## Contributing

We welcome contributions! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
