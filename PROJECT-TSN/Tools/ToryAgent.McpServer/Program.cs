using System;
using System.Threading;
using ToryAgent.McpServer.Application;
using ToryAgent.McpServer.Tools;
using ToryAgent.McpServer.Transport;

CancellationTokenSource cts = new();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

UnityBridgeClient unityBridgeClient = new();

ToolRegistry toolRegistry = new(new IMcpTool[]
{
    // ── Built-in ────────────────────────────────────────────────────────────
    new EchoTool(),
    new GetServerStatusTool(),

    // ── Skills ──────────────────────────────────────────────────────────────
    new ListSkillsTool(),
    new ReadSkillTool(),
    new CreateSkillTool(),
    new DeleteSkillTool(),

    // ── Selection ───────────────────────────────────────────────────────────
    new GetSelectionProxyTool(unityBridgeClient),

    // ── Scene ───────────────────────────────────────────────────────────────
    new GetSceneInfoProxyTool(unityBridgeClient),
    new GetSceneHierarchyProxyTool(unityBridgeClient),
    new SaveSceneProxyTool(unityBridgeClient),

    // ── GameObject Query ────────────────────────────────────────────────────
    new GetGameObjectDetailsProxyTool(unityBridgeClient),
    new FindGameObjectsProxyTool(unityBridgeClient),
    new GetComponentsProxyTool(unityBridgeClient),

    // ── GameObject Create / Modify ──────────────────────────────────────────
    new CreateGameObjectProxyTool(unityBridgeClient),
    new CreatePrimitiveProxyTool(unityBridgeClient),
    new SetTransformProxyTool(unityBridgeClient),
    new DeleteGameObjectProxyTool(unityBridgeClient),
    new SetGameObjectActiveProxyTool(unityBridgeClient),
    new SetGameObjectNameProxyTool(unityBridgeClient),
    new SetGameObjectParentProxyTool(unityBridgeClient),
    new DuplicateGameObjectProxyTool(unityBridgeClient),

    // ── Component ───────────────────────────────────────────────────────────
    new AddComponentProxyTool(unityBridgeClient),
    new SetComponentPropertyProxyTool(unityBridgeClient),
    new SetGameObjectTagProxyTool(unityBridgeClient),
    new SetSerializedFieldProxyTool(unityBridgeClient),

    // ── Material ────────────────────────────────────────────────────────────
    new CreateMaterialProxyTool(unityBridgeClient),
    new AssignMaterialProxyTool(unityBridgeClient),
    new SetMaterialColorProxyTool(unityBridgeClient),

    // ── UI ──────────────────────────────────────────────────────────────────
    new CreateCanvasProxyTool(unityBridgeClient),
    new CreateUIElementProxyTool(unityBridgeClient),
    new SetRectTransformProxyTool(unityBridgeClient),
    new SetUITextProxyTool(unityBridgeClient),
    new SetUIColorProxyTool(unityBridgeClient),

    // ── Assets ──────────────────────────────────────────────────────────────
    new ListAssetsProxyTool(unityBridgeClient),
    new CreateFolderProxyTool(unityBridgeClient),
});

RequestRouter router = new(toolRegistry);
StdioServer server = new(router);

await server.RunAsync(cts.Token);
