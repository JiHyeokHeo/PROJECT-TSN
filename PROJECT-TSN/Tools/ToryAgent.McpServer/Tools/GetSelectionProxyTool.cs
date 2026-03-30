using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ToryAgent.McpServer.Application;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

public sealed class GetSelectionProxyTool : IMcpTool
{
    readonly UnityBridgeClient unityBridgeClient;

    public GetSelectionProxyTool(UnityBridgeClient unityBridgeClient)
    {
        this.unityBridgeClient = unityBridgeClient;
    }

    public string Name => "get_selection";

    public string Description => "Returns current Unity selection from the Unity Editor.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject(),
        ["additionalProperties"] = false
    };

    public async Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        string argumentsJson = arguments?.ToJsonString() ?? "{}";

        UnityBridgeExecuteResponse response = await unityBridgeClient.ExecuteToolAsync(
            Name,
            argumentsJson,
            cancellationToken);

        ToolCallResult result = new()
        {
            Content = new object[]
            {
                new
                {
                    type = "text",
                    text = response.ResultJson
                }
            },
            IsError = false
        };

        return result;
    }
}