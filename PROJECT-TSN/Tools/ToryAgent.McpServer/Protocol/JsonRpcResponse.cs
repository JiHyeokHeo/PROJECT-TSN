using System.Text.Json.Serialization;

namespace ToryAgent.McpServer.Protocol;

public sealed class JsonRpcResponse
{
    public string Jsonrpc { get; set; } = "2.0";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; set; }

    public static JsonRpcResponse FromResult(object? id, object? result)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Result = result
        };
    }

    public static JsonRpcResponse FromError(object? id, int code, string message)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message
            }
        };
    }
}
