namespace ToryAgent.McpServer.Protocol;

public sealed class ToolCallResult
{
    public object[] Content { get; set; } = Array.Empty<object>();
    public bool IsError { get; set; }
}