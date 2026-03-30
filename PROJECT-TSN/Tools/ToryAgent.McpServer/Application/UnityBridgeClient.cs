using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ToryAgent.McpServer.Application;

public sealed class UnityBridgeClient
{
    readonly HttpClient httpClient;
    readonly string baseUrl;

    public UnityBridgeClient(string baseUrl = "http://127.0.0.1:63211")
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<UnityBridgeListToolsResponse> ListToolsAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            $"{baseUrl}/tory/tools/list",
            cancellationToken);

        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Unity bridge list failed: {json}");

        UnityBridgeListToolsResponse? result = JsonSerializer.Deserialize<UnityBridgeListToolsResponse>(
            json,
            CreateJsonOptions());

        return result ?? new UnityBridgeListToolsResponse();
    }

    public async Task<UnityBridgeExecuteResponse> ExecuteToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken)
    {
        UnityBridgeExecuteRequest request = new()
        {
            ToolName = toolName,
            ArgumentsJson = string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson
        };

        string requestJson = JsonSerializer.Serialize(request, CreateJsonOptions());

        using StringContent content = new(requestJson, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync(
            $"{baseUrl}/tory/tools/execute",
            content,
            cancellationToken);

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        UnityBridgeExecuteResponse? result = JsonSerializer.Deserialize<UnityBridgeExecuteResponse>(
            responseJson,
            CreateJsonOptions());

        if (result == null)
            throw new InvalidOperationException("Unity bridge returned empty response.");

        if (!response.IsSuccessStatusCode || !result.Success)
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Unity bridge execution failed." : result.ErrorMessage);

        return result;
    }

    static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}

public sealed class UnityBridgeExecuteRequest
{
    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("argumentsJson")]
    public string ArgumentsJson { get; set; } = "{}";
}

public sealed class UnityBridgeExecuteResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("resultJson")]
    public string ResultJson { get; set; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;
}

public sealed class UnityBridgeListToolsResponse
{
    [JsonPropertyName("tools")]
    public UnityBridgeToolInfo[] Tools { get; set; } = Array.Empty<UnityBridgeToolInfo>();
}

public sealed class UnityBridgeToolInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchemaJson")]
    public string InputSchemaJson { get; set; } = "{}";
}