using MCPSharp.Model;
using MCPSharp.Model.Schemas;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace MCPSharp.Core.Tools
{
    class ResourceManager
    {
        public readonly List<Resource> Resources = [];
        public void Register<T>() where T : class, new()
        {
            var type = typeof(T);

            foreach (var method in type.GetMethods())
            {
                var resAttr = method.GetCustomAttribute<McpResourceAttribute>();
                if (resAttr != null)
                {
                    Resources.Add(new Resource()
                    {
                        Name = resAttr.Name,
                        Description = resAttr.Description,
                        Uri = resAttr.Uri,
                        MimeType = resAttr.MimeType
                    });
                }
            }

            foreach (var property in type.GetProperties())
            {

                var resAttr = property.GetCustomAttribute<McpResourceAttribute>();
                if (resAttr != null)
                {
                    Resources.Add(new Resource()
                    {
                        Name = resAttr.Name,
                        Description = resAttr.Description,
                        Uri = resAttr.Uri,
                        MimeType = resAttr.MimeType
                    });
                }
            }
        }
    }
    class ToolManager
    {

        public readonly Dictionary<string, ToolHandler> Tools = [];

        //triggertoolchahge notification when a tool is added
        public Action ToolChangeNotification = () => { };

        /// <summary>
        /// Scans a class for Tools and resources and registers them with the server
        /// </summary>
        public void Register<T>() where T : class, new()
        {

            var type = typeof(T);

            foreach (var method in type.GetMethods())
            {
                RegisterMcpFunction(method);
                RegisterSemanticKernelFunction(method);
            }

            ToolChangeNotification.Invoke();
        }

        public async Task RegisterAIFunctionAsync(AIFunction function)
        {
            Tools[function.Name] = new ToolHandler(new Tool
            {
                Name = function.Name,
                Description = function.Description,
                InputSchema = JsonSerializer.Deserialize<InputSchema>(function.JsonSchema)
            }, function.UnderlyingMethod); // ¯\_(ツ)_/¯
            await Task.Run(ToolChangeNotification.Invoke);
        }

        public void AddToolHandler(ToolHandler tool)
        {
            Tools[tool.Tool.Name] = tool;
            ToolChangeNotification.Invoke();
        }

        private void RegisterSemanticKernelFunction(MethodInfo method)
        {
            var kernelFunctionAttribute = method.GetCustomAttribute<KernelFunctionAttribute>();
            if (kernelFunctionAttribute == null) return;

            var parameters = method.GetParameters();

            Dictionary<string, ParameterSchema> parameterSchemas = [];

            foreach (var parameter in parameters)
            {
                parameterSchemas.Add(parameter.Name, GetParameterSchema(parameter));
            }


            Tools[kernelFunctionAttribute.Name] = new ToolHandler(new Tool
            {
                Name = kernelFunctionAttribute.Name,
                Description = method.GetCustomAttribute<DescriptionAttribute>().Description ?? "",
                InputSchema = new InputSchema
                {
                    Properties = parameterSchemas,
                    Required = [.. parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key)],
                }
            }, method!);
        }

        private static ParameterSchema GetParameterSchema(ParameterInfo parameter)
        {
            string type = parameter.ParameterType switch
            {
                Type t when t == typeof(string) => "string",
                Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) => "number",
                Type t when t == typeof(bool) => "boolean",
                Type t when t.IsArray => "array",
                Type t when t == typeof(DateTime) => "string",
                _ => "object"
            };

            var schema = new ParameterSchema
            {
                Type = type,
                Description = parameter.GetXmlDocumentation() ?? parameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                Required = parameter.GetCustomAttribute<RequiredAttribute>() != null,
                Contents = type == "object" ? AIJsonUtilities.CreateJsonSchema(parameter.ParameterType) : null
            } ;

            return schema;
        }

        private void RegisterMcpFunction(MethodInfo method)
        {
            string name = "";
            string description = "";

#pragma warning disable CS0618 // This is needed for backwards compatibility with older versions of the library
            var mcpFuncAttr = method.GetCustomAttribute<McpFunctionAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
            if (mcpFuncAttr != null)
            {
                name = mcpFuncAttr.Name ?? method.Name;
                description = mcpFuncAttr.Description ?? method.GetXmlDocumentation();
            }
            else
            {
                var methodAttr = method.GetCustomAttribute<McpToolAttribute>();
                if (methodAttr != null)
                {
                    name = methodAttr.Name ?? method.Name;
                    description = methodAttr.Description ?? method.GetXmlDocumentation();
                }
                else { return; }
            }

            Dictionary<string, ParameterSchema> parameterSchemas = [];

            foreach (var parameter in method.GetParameters())
            {
                parameterSchemas.Add(parameter.Name, GetParameterSchema(parameter));
            }

            Tools[name] = new ToolHandler(new Tool
            {
                Name = name,
                Description = description ?? "",
                InputSchema = new InputSchema
                {
                    Properties = parameterSchemas,
                    Required = [.. parameterSchemas.Where(kvp => kvp.Value.Required).Select(kvp => kvp.Key)],
                }
            }, method!);
        }
    }
}
