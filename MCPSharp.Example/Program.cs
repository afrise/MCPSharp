using MCPSharp;
using MCPSharp.Model;
using MCPSharp.Model.Schemas;
using MCPSharp.Example.Import;
using Microsoft.Extensions.AI;

//register tools that are not part of the core assembly
MCPServer.Register<ExternalTool>();

//register tools that are build with semantic kernel attributes
MCPServer.Register<SemKerExample>();

//example AIFunctions created from lambdas
var myFunc = AIFunctionFactory.Create(() => { return "ahoyhoy!"; }, "AI_Function", "an AIFunction that has been imported");
var myOtherFunc = AIFunctionFactory.Create((string input) => { return input.ToUpperInvariant(); }, "to_upper", "converts a string to uppercase (culture invariant)");

//register the AIFunctions
MCPServer.RegisterAIFunction(myFunc);
MCPServer.RegisterAIFunction(myOtherFunc);

//add a dynamically built tool handler 
MCPServer.AddToolHandler( new Tool() 
{
    Name = "dynamicTool",
    Description = "A Test Tool",
    InputSchema = new InputSchema {
        Type = "object",
        Required = ["input"],
        Properties = new Dictionary<string, ParameterSchema>{
            {"input", new ParameterSchema{Type="string", Description="the input"}},
            {"input2", new ParameterSchema{Type="string", Description="the input2"}}
        }
    }
}, (string input, string? input2 = null) => { return $"hello, {input}.\n{input2 ?? 
    "didn't feel like filling in the second value just because it wasn't required? shame. just kidding! thanks for your help!"}"; });

//start the server
await MCPServer.StartAsync("TestServer", "1.0");