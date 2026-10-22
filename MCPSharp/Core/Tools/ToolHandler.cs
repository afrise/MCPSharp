using MCPSharp.Model;
using MCPSharp.Model.Content;
using MCPSharp.Model.Results;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core.Tools
{
    public class ToolHandler(Tool tool, MethodInfo method)
    {
        public Tool Tool = tool;
        private readonly MethodInfo _method = method;
       
        public async Task<CallToolResult> HandleAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                
                var inputValues = new Dictionary<string, object>();
                foreach (var par in _method.GetParameters())
                {
                    if (parameters.TryGetValue(par.Name, out var value)) {
                        if (value is JsonElement element)
                        {
                            value = JsonSerializer.Deserialize(element.GetRawText(), par.ParameterType);
                        }

                        inputValues.Add(par.Name, value);
                    }
                    else
                    {
                        inputValues.Add(par.Name, par.HasDefaultValue
                            ? par.DefaultValue
                            : par.ParameterType.IsValueType ? Activator.CreateInstance(par.ParameterType) : null);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return new CallToolResult { IsError = true, Content = [new TextContent("Operation was cancelled")] };
                }

                var result = _method.Invoke(Activator.CreateInstance(_method.DeclaringType),[.. inputValues.Values]);


                if (cancellationToken.IsCancellationRequested)
                {
                    return new CallToolResult { IsError = true, Content = [new TextContent("Operation was cancelled")] };
                }

                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }

                if (result is string resultString)
                    return new CallToolResult { Content = [new (resultString)]};
                
                if (result is string[] resultStringArray)
                    return new CallToolResult { Content = [.. resultStringArray.Select(s => new TextContent(s))] };

                if (result is null)
                {
                    return new CallToolResult { IsError = true, Content = [new("null")] };
                }

                if (result is JsonElement jsonElement)
                {
                    return new CallToolResult { Content = [new(jsonElement.GetRawText())] };
                }   

                else return new CallToolResult { Content = [new(result.ToString())] };
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
