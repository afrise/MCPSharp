using System.Reflection;

namespace MCPSharp.Core
{
    class ResourceManager
    {
        public readonly List<Resource> Resources = [];

        public void Register(object instance)
        {
            var type = instance.GetType();
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
                        MimeType = resAttr.MimeType,
                        Method = method,
                        Instance = instance
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
                        MimeType = resAttr.MimeType,
                        Method = property.GetMethod,
                        Instance = instance
                    });
                }
            }
        }
    }
}
