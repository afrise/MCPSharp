using System;
using System.Threading.Tasks;
using MCPSharp;
using MCPSharp.Model;
using MCPSharp.Model.Schemas;

namespace MCPSharp.Example.BackgroundServer;

/// <summary>
/// This example demonstrates how to run an MCP server in a background thread using MCPServerHost.
/// The server is started asynchronously and will continue to run even after the main thread exits.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the MCP server example.
    /// </summary>   
    public static async Task Main()
    {
        // Start server in background thread
        await using var server = await MCPServerHost.StartAsync("BackgroundServer", "1.0.0");
            
        // Add a sample tool
        MCPServer.AddToolHandler(new Tool()
        {
            Name = "greet",
            Description = "A simple greeting tool",
            InputSchema = new InputSchema
            {
                Type = "object",
                Required = ["name"],
                Properties = new Dictionary<string, ParameterSchema>{
                    {"name", new ParameterSchema{Type="string", Description="Name to greet"}}
                }
            }
        }, (string name) => $"Hello, {name}! (from background server)");

        await Task.Delay(-1);
    }
}
