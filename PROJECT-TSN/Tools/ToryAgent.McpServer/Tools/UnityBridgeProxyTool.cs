using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ToryAgent.McpServer.Application;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

public abstract class UnityBridgeProxyTool : IMcpTool
{
    protected readonly UnityBridgeClient unityBridgeClient;

    protected UnityBridgeProxyTool(UnityBridgeClient unityBridgeClient)
    {
        this.unityBridgeClient = unityBridgeClient;
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract JsonObject InputSchema { get; }

    public async Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        string argumentsJson = arguments?.ToJsonString() ?? "{}";

        UnityBridgeExecuteResponse response = await unityBridgeClient.ExecuteToolAsync(
            Name,
            argumentsJson,
            cancellationToken);

        return new ToolCallResult
        {
            Content = new object[]
            {
                new { type = "text", text = response.Success ? response.ResultJson : response.ErrorMessage }
            },
            IsError = !response.Success
        };
    }
}
