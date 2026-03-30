using System.Text.Json.Nodes;
using ToryAgent.McpServer.Protocol;
using ToryAgent.McpServer.Tools;

namespace ToryAgent.McpServer.Application;

public sealed class RequestRouter
{
    readonly ToolRegistry toolRegistry;

    public RequestRouter(ToolRegistry toolRegistry)
    {
        this.toolRegistry = toolRegistry;
    }

    public async Task<JsonRpcResponse?> RouteAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return JsonRpcResponse.FromError(null, -32600, "Invalid Request");

        if (string.IsNullOrWhiteSpace(request.Method))
            return JsonRpcResponse.FromError(request.Id, -32600, "Method is missing");

        try
        {
            return request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "notifications/initialized" => null,
                "tools/list" => HandleToolsList(request),
                "tools/call" => await HandleToolsCallAsync(request, cancellationToken),
                _ => JsonRpcResponse.FromError(request.Id, -32601, $"Method not found: {request.Method}")
            };
        }
        catch (ArgumentException ex)
        {
            return JsonRpcResponse.FromError(request.Id, -32602, ex.Message);
        }
        catch (Exception ex)
        {
            return JsonRpcResponse.FromError(request.Id, -32000, ex.Message);
        }
    }

    JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        InitializeResult result = new()
        {
            ProtocolVersion = "2025-03-26",
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolCapability
                {
                    ListChanged = false
                }
            },
            ServerInfo = new ServerIdentity
            {
                Name = ServerInfo.Name,
                Version = ServerInfo.Version
            }
        };

        return JsonRpcResponse.FromResult(request.Id, result);
    }

    JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        ToolDefinition[] tools = toolRegistry.GetAll()
            .Select(tool => new ToolDefinition
            {
                Name = tool.Name,
                Description = tool.Description,
                InputSchema = tool.InputSchema
            })
            .ToArray();

        var result = new
        {
            tools
        };

        return JsonRpcResponse.FromResult(request.Id, result);
    }

    async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        JsonObject? paramsObject = request.Params;
        if (paramsObject == null)
            throw new ArgumentException("Params are missing.");

        string? toolName = paramsObject["name"]?.GetValue<string>();
        JsonObject arguments = paramsObject["arguments"] as JsonObject ?? new JsonObject();

        if (string.IsNullOrWhiteSpace(toolName))
            throw new ArgumentException("Tool name is missing.");

        if (!toolRegistry.TryGet(toolName, out IMcpTool? tool) || tool == null)
            throw new ArgumentException($"Tool not found: {toolName}");

        ToolCallResult toolResult = await tool.ExecuteAsync(arguments, cancellationToken);
        return JsonRpcResponse.FromResult(request.Id, toolResult);
    }
}