using System.Text.Json.Nodes;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    JsonObject InputSchema { get; }

    Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken);
}