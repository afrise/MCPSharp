using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;

JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

McpServerConfigurationCollection conf = JsonSerializer.Deserialize<McpServerConfigurationCollection>(File.ReadAllText("config.json"), 
    jsonSerializerOptions)!;

MCPClientPool clients = [];
foreach (var server in conf.McpServers)
{
    clients.Add(server.Key, server.Value, 
        (parameters) =>
        { //this is an example permission function, you can replace it with your own. just press y when prompted to allow the tool to run
            Console.WriteLine("The Assistant wants to run a tool.");
        Console.WriteLine($"\tTool: {parameters["tool"]}");
        Console.WriteLine($"\tParameters:");
        Console.WriteLine(Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(parameters["parameters"])));
        Console.WriteLine("Do you want to allow it? (y/N)");
        return Console.ReadKey().Key == ConsoleKey.Y;
   });
}

var chatOptions = new ChatOptions { 
    Tools = clients.GetAllAIFunctions(),
    ToolMode = ChatToolMode.Auto //let the assistant choose not to use a tool if it doesn't need to
};
var chatHistory = new List<ChatMessage>() { new(ChatRole.System, conf.Models["ollama"].SystemPrompt) };
var chatClient = new OllamaSharp.OllamaApiClient(conf.Models["ollama"].Endpoint, conf.Models["ollama"].ModelId); //.AsBuilder().UseFunctionInvocation().Build();


while (true)
{
    Console.Write("\n\n[User] >> ");
    var input = Console.ReadLine();
    if (input == "bye") break;
    if (string.IsNullOrWhiteSpace(input)) continue;
    var response = await chatClient.GetResponseAsync(input, chatOptions);
    Console.WriteLine($"\n\n[Assistant] {DateTime.Now.ToShortTimeString()}: {response}");
    chatHistory.Add(response.Messages.First());
}

class McpServerConfiguration
{
    public required string Command { get; set; }
    public string[] Args { get; set; } = [];
    public Dictionary<string, string> Env { get; set; } = [];
}

class McpServerConfigurationCollection
{
    public Dictionary<string, McpServerConfiguration> McpServers { get; set; }
    public Dictionary<string, ModelConfiguration> Models { get; set; }
}


class ModelConfiguration
{
    public string Endpoint { get; set; }
    public string ModelId { get; set; }
    public string SystemPrompt { get; set; }
}

