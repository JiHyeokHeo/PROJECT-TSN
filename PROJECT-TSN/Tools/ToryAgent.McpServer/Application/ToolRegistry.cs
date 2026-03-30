using ToryAgent.McpServer.Tools;

namespace ToryAgent.McpServer.Application;

public sealed class ToolRegistry
{
    readonly Dictionary<string, IMcpTool> toolMap = new(StringComparer.Ordinal);

    public ToolRegistry(IEnumerable<IMcpTool> tools)
    {
        foreach (IMcpTool tool in tools)
            toolMap[tool.Name] = tool;
    }

    public IReadOnlyCollection<IMcpTool> GetAll()
    {
        return toolMap.Values;
    }

    public bool TryGet(string name, out IMcpTool? tool)
    {
        return toolMap.TryGetValue(name, out tool);
    }
}