using MCPSharp.Model;
using MCPSharp.Model.Content;
using MCPSharp.Model.Results;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core
{
    internal class ToolHandler<T>(Tool tool, MethodInfo method, T instance) where T : class, new()
    {
        public Tool Tool = tool;
        private readonly MethodInfo _method = method;
        private readonly T _instance = instance;
       
        public async Task<CallToolResult> HandleAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                
                var inputValues = new Dictionary<string, object>();

                foreach (var item in parameters)
                {
                    if (item.Value is JsonElement jsonElement)
                    {
                        inputValues.Add(item.Key, JsonSerializer.Deserialize(jsonElement.GetRawText()!, _method.GetParameters().FirstOrDefault(p => p.Name == item.Key)!.ParameterType!)!);
                    }
                }

                var result = _method.Invoke(_instance, [.. inputValues.Values]);

                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }

                return new CallToolResult
                {
                    Content = [new TextContent { Type = "text", Text = result?.ToString() ?? "no response" }]
                };
            }
            catch (Exception ex)
            {
                var e = ex is TargetInvocationException tie ? tie.InnerException ?? tie : ex;
                var stackTrace = e.StackTrace?.Split([Environment.NewLine], StringSplitOptions.None)
                                              .Where(line => !line.Contains("System.RuntimeMethodHandle.InvokeMethod") 
                                                          && !line.Contains("System.Reflection.MethodBaseInvoker.InvokeWithNoArgs")).ToArray();

                return new CallToolResult
                {
                    IsError = true,
                    Content =
                    [
                        new TextContent { Text = $"{e.Message}" },
                        new TextContent { Text = $"StackTrace:\n{string.Join("\n", stackTrace)}" }
                    ]
                };
            }
        }
    }
}
