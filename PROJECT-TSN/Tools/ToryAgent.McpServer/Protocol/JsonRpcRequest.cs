using System.Text.Json.Nodes;

namespace ToryAgent.McpServer.Protocol;

public sealed class JsonRpcRequest
{
    public string Jsonrpc { get; set; } = "2.0";
    public JsonNode? Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public JsonObject? Params { get; set; }
}