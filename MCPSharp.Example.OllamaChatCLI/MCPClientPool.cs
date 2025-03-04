using MCPSharp;
using Microsoft.Extensions.AI;
using System.Collections;

class MCPClientPool : ICollection<MCPClient>
{
    private readonly List<MCPClient> clients = [];

    public async Task<List<AITool>> GetAllAIFunctionsAsync()
    {
        var functions = new List<AITool>();
        
        foreach (var c in clients)
        {
            functions.AddRange(await c.GetFunctionsAsync());
        }
        
        return functions;
    }

    public int Count => clients.Count;
    public bool IsReadOnly => false;
    public void Add(string name, McpServerConfiguration server, Func<Dictionary<string, object>, bool>? permissionFunction = null)
    {
        clients.Add(new MCPClient(name, "0.1.0", server.Command, string.Join(' ', server.Args ?? []), server.Env)
        {
            GetPermission = permissionFunction ?? ((parameters) => true)
        });
    }

    public void Add(MCPClient item) => clients.Add(item);
    public void Clear() => clients.Clear();
    public bool Contains(MCPClient item) => clients.Contains(item);
    public void CopyTo(MCPClient[] array, int arrayIndex) => clients.CopyTo(array, arrayIndex);
    public IEnumerator<MCPClient> GetEnumerator() => clients.GetEnumerator();
    public bool Remove(MCPClient item) => clients.Remove(item);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}