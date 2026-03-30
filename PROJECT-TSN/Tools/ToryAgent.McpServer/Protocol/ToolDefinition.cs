using System.Text.Json.Nodes;

namespace ToryAgent.McpServer.Protocol;

public sealed class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonObject InputSchema { get; set; } = new();
}