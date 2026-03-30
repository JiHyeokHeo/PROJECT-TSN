namespace ToryAgent.McpServer.Protocol;

public sealed class JsonRpcError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}