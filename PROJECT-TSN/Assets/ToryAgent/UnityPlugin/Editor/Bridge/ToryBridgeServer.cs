using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class ToryBridgeServer : IDisposable
    {
        public const int DefaultPort = 63211;

        readonly HttpListener listener = new();
        readonly UnityToolRegistry toolRegistry;

        CancellationTokenSource cancellationTokenSource;
        Task serverLoopTask;

        public bool IsRunning => listener.IsListening;
        public int Port { get; }

        public ToryBridgeServer(UnityToolRegistry toolRegistry, int port = DefaultPort)
        {
            this.toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            Port = port;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            string prefix = $"http://127.0.0.1:{Port}/";
            listener.Prefixes.Clear();
            listener.Prefixes.Add(prefix);
            listener.Start();

            serverLoopTask = Task.Run(() => RunLoopAsync(cancellationTokenSource.Token));
            UnityEngine.Debug.Log($"[ToryBridge] Started on {prefix}");
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            try
            {
                cancellationTokenSource.Cancel();
                listener.Stop();
                listener.Close();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("[ToryBridge] Stop exception: " + ex.Message);
            }

            UnityEngine.Debug.Log("[ToryBridge] Stopped");
        }

        async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;

                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning("[ToryBridge] Listener error: " + ex.Message);
                    break;
                }

                _ = Task.Run(() => HandleContextAsync(context), cancellationToken);
            }
        }

        async Task HandleContextAsync(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url.AbsolutePath;

                if (context.Request.HttpMethod == "GET" && path == "/tory/tools/list")
                {
                    await HandleListToolsAsync(context);
                    return;
                }

                if (context.Request.HttpMethod == "POST" && path == "/tory/tools/execute")
                {
                    await HandleExecuteToolAsync(context);
                    return;
                }

                context.Response.StatusCode = 404;
                await WriteTextResponseAsync(context.Response, "Not Found", "text/plain");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[ToryBridge] Request handling failed: " + ex);
                if (context.Response.OutputStream.CanWrite)
                {
                    context.Response.StatusCode = 500;
                    await WriteJsonResponseAsync(context.Response, new ToryBridgeResponse
                    {
                        success = false,
                        resultJson = "",
                        errorMessage = ex.Message
                    });
                }
            }
        }

        Task HandleListToolsAsync(HttpListenerContext context)
        {
            var tools = toolRegistry.GetAll();

            var payload = new
            {
                tools = tools.Select(tool => new
                {
                    name = tool.Name,
                    description = tool.Description,
                    inputSchemaJson = tool.InputSchemaJson
                }).ToArray()
            };

            return WriteJsonResponseAsync(context.Response, payload);
        }

        async Task HandleExecuteToolAsync(HttpListenerContext context)
        {
            string body;
            using (StreamReader reader = new(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            ToryBridgeRequest request = JsonConvert.DeserializeObject<ToryBridgeRequest>(body);
            if (request == null || string.IsNullOrWhiteSpace(request.toolName))
            {
                context.Response.StatusCode = 400;
                McpBridgeLog.AddEntry(new McpLogEntry
                {
                    Timestamp = DateTime.Now,
                    ToolName = "(unknown)",
                    ArgumentsJson = "{}",
                    Success = false,
                    ErrorMessage = "toolName is missing.",
                    DurationMs = 0
                });
                await WriteJsonResponseAsync(context.Response, new ToryBridgeResponse
                {
                    success = false,
                    resultJson = "",
                    errorMessage = "toolName is missing."
                });
                return;
            }

            if (!toolRegistry.TryGet(request.toolName, out IUnityEditorTool tool))
            {
                context.Response.StatusCode = 404;
                McpBridgeLog.AddEntry(new McpLogEntry
                {
                    Timestamp = DateTime.Now,
                    ToolName = request.toolName,
                    ArgumentsJson = request.argumentsJson ?? "{}",
                    Success = false,
                    ErrorMessage = $"Tool not found: {request.toolName}",
                    DurationMs = 0
                });
                await WriteJsonResponseAsync(context.Response, new ToryBridgeResponse
                {
                    success = false,
                    resultJson = "",
                    errorMessage = $"Tool not found: {request.toolName}"
                });
                return;
            }

            var sw = Stopwatch.StartNew();
            string resultJson;
            bool execSuccess;
            string execError = "";

            try
            {
                resultJson = await ExecuteOnMainThreadAsync(() =>
                    tool.Execute(string.IsNullOrWhiteSpace(request.argumentsJson) ? "{}" : request.argumentsJson));
                execSuccess = true;
            }
            catch (Exception ex)
            {
                resultJson = "";
                execSuccess = false;
                execError = ex.Message;
                UnityEngine.Debug.LogError($"[ToryBridge] Tool '{request.toolName}' threw: {ex}");
            }

            sw.Stop();

            McpBridgeLog.AddEntry(new McpLogEntry
            {
                Timestamp = DateTime.Now,
                ToolName = request.toolName,
                ArgumentsJson = request.argumentsJson ?? "{}",
                ResultJson = resultJson,
                Success = execSuccess,
                ErrorMessage = execError,
                DurationMs = sw.ElapsedMilliseconds
            });

            if (execSuccess)
            {
                await WriteJsonResponseAsync(context.Response, new ToryBridgeResponse
                {
                    success = true,
                    resultJson = resultJson,
                    errorMessage = ""
                });
            }
            else
            {
                context.Response.StatusCode = 500;
                await WriteJsonResponseAsync(context.Response, new ToryBridgeResponse
                {
                    success = false,
                    resultJson = "",
                    errorMessage = execError
                });
            }
        }

        static Task<string> ExecuteOnMainThreadAsync(Func<string> action)
        {
            TaskCompletionSource<string> tcs = new();

            EditorApplication.CallbackFunction handler = null;
            handler = () =>
            {
                EditorApplication.update -= handler;
                try
                {
                    tcs.TrySetResult(action());
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            EditorApplication.update += handler;

            return tcs.Task;
        }

        static async Task WriteJsonResponseAsync(HttpListenerResponse response, object payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            await WriteTextResponseAsync(response, json, "application/json");
        }

        static async Task WriteTextResponseAsync(HttpListenerResponse response, string text, string contentType)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            response.ContentType = contentType;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;

            using Stream output = response.OutputStream;
            await output.WriteAsync(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            Stop();
            cancellationTokenSource?.Dispose();
        }
    }
}