using System.Text.Json.Nodes;
using ToryAgent.McpServer.Application;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

public sealed class GetServerStatusTool : IMcpTool
{
    public string Name => "get_server_status";

    public string Description => "Returns basic ToryAgent MCP server status.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject()
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        ToolCallResult result = new()
        {
            Content = new object[]
            {
                new
                {
                    type = "text",
                    text = $"Server '{ServerInfo.Name}' is running. Version: {ServerInfo.Version}"
                }
            },
            IsError = false
        };

        return Task.FromResult(result);
    }
}