using System.Text.Json.Nodes;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

public sealed class EchoTool : IMcpTool
{
    public string Name => "echo";

    public string Description => "Returns the same text that was provided.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["text"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Text to echo back"
            }
        },
        ["required"] = new JsonArray("text")
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        string? text = arguments["text"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Missing required argument: text");

        ToolCallResult result = new()
        {
            Content = new object[]
            {
                new
                {
                    type = "text",
                    text = text
                }
            },
            IsError = false
        };

        return Task.FromResult(result);
    }
}