using MCPSharp.Core.Tools;
using MCPSharp.Model;

namespace MCPSharp.Test;

[TestClass]
public class ToolHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_UsesDeclaredDefaultForMissingOptionalString()
    {
        var method = GetType().GetMethod(nameof(OptionalString))!;
        var handler = new ToolHandler(new Tool { Name = method.Name }, method);

        var result = await handler.HandleAsync(new Dictionary<string, object>());

        Assert.IsFalse(result.IsError);
        Assert.AreEqual("fallback", result.Content[0].Text);
    }

    [TestMethod]
    public async Task HandleAsync_UsesDeclaredDefaultForMissingOptionalValueType()
    {
        var method = GetType().GetMethod(nameof(OptionalCount))!;
        var handler = new ToolHandler(new Tool { Name = method.Name }, method);

        var result = await handler.HandleAsync(new Dictionary<string, object>());

        Assert.IsFalse(result.IsError);
        Assert.AreEqual("7", result.Content[0].Text);
    }

    public string OptionalString(string value = "fallback") => value;

    public int OptionalCount(int count = 7) => count;
}
