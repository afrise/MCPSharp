using MCPSharp.Model;
using MCPSharp.Model.Content;
using MCPSharp.Model.Results;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core.Tools
{
    /// <summary>
    /// The ToolHandler class is responsible for handling the invocation of a tool.
    /// </summary>
    /// <param name="tool">The Tool object holds the description of the Tool and it's parameters</param>
    /// <param name="method">The Attributes and metadata of a method, needed to invoke it.</param>
    /// <param name="instance">The instance of the object that contains the method to be invoked.</param>
    public class ToolHandler(Tool tool, MethodInfo method, object instance = null)
    {
        /// <summary>
        /// The Tool object holds the description of the Tool and it's parameters
        /// </summary>
        public Tool Tool = tool;
        
        private readonly MethodInfo _method = method;

        private readonly object _instance = instance ?? Activator.CreateInstance(method.DeclaringType);

        /// <summary>
        /// Handles the invocation of a tool with the specified parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
                        inputValues.Add(par.Name, par.ParameterType.IsValueType ? Activator.CreateInstance(par.ParameterType) : null);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return new CallToolResult { IsError = true, Content = [new TextContent("Operation was cancelled")] };
                }

                var result = _method.Invoke(_instance, [.. inputValues.Values]);


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
