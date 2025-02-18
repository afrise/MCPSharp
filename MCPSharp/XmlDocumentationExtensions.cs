using System.Reflection;
using System.Xml.Linq;

namespace MCPSharp
{
    internal static class XmlDocumentationExtensions
    {
        public static string GetXmlDocumentation(this MemberInfo member)
        {
            try
            {
                var assemblyName = member.DeclaringType?.Assembly.GetName().Name;
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");

                if (!File.Exists(xmlPath)) return null;

                var doc = XDocument.Load(xmlPath);
                var memberName = member switch
                {
                    MethodInfo method => GetMethodString(method),
                    PropertyInfo property => $"P:{property.DeclaringType?.FullName}.{property.Name}",
                    FieldInfo field => $"F:{field.DeclaringType?.FullName}.{field.Name}",
                    TypeInfo type => $"T:{type.FullName}",
                    _ => $"T:{member.DeclaringType?.FullName}"
                };

                return doc.Descendants("member")
                         .FirstOrDefault(m => m.Attribute("name")?.Value == memberName)
                         ?.Element("summary")?.Value.Trim();
            }
            catch
            {
                return null;
            }
        }

        public static string GetXmlDocumentation(this ParameterInfo parameter)
        {
            try
            {
                var assemblyName = parameter.Member.DeclaringType?.Assembly.GetName().Name;
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");

                if (!File.Exists(xmlPath)) return null;

                var doc = XDocument.Load(xmlPath);
                var method = parameter.Member as MethodInfo;
                if (method == null) return null;

                return doc.Descendants("member").FirstOrDefault(m => m.Attribute("name")?.Value == GetMethodString(method))
                         ?.Elements("param").FirstOrDefault(p => p.Attribute("name")?.Value == parameter.Name)
                         ?.Value.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string GetMethodString(MethodInfo method) => $"M:{method.DeclaringType?.FullName}.{method.Name}{GetParameterString(method)}";
        private static string GetParameterString(MethodInfo method) => method.GetParameters().Length == 0
                    ? string.Empty : "(" + string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName)) + ")";
    }
}