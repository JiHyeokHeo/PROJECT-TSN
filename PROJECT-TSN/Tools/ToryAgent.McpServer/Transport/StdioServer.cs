using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToryAgent.McpServer.Application;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Transport;

public sealed class StdioServer
{
    readonly RequestRouter router;
    readonly JsonSerializerOptions jsonOptions;

    public StdioServer(RequestRouter router)
    {
        this.router = router;
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line = await Console.In.ReadLineAsync();
            if (line == null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            JsonRpcResponse? response;

            try
            {
                JsonRpcRequest? request = JsonSerializer.Deserialize<JsonRpcRequest>(line, jsonOptions);

                if (request == null)
                {
                    response = JsonRpcResponse.FromError(null, -32600, "Invalid Request");
                }
                else
                {
                    response = await router.RouteAsync(request, cancellationToken);
                }
            }
            catch (JsonException ex)
            {
                response = JsonRpcResponse.FromError(null, -32700, $"Parse error: {ex.Message}");
            }
            catch (Exception ex)
            {
                response = JsonRpcResponse.FromError(null, -32000, ex.Message);
            }

            if (response == null)
                continue;

            string responseJson = JsonSerializer.Serialize(response, jsonOptions);
            await Console.Out.WriteLineAsync(responseJson);
            await Console.Out.FlushAsync();
        }
    }
}
