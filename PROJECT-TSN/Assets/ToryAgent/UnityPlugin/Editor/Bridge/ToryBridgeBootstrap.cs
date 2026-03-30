using UnityEditor;
using UnityEngine;

namespace ToryAgent.UnityPlugin.Editor
{
    [InitializeOnLoad]
    public static class ToryBridgeBootstrap
    {
        static ToryBridgeServer server;

        static ToryBridgeBootstrap()
        {
            EditorApplication.delayCall += StartServer;
            AssemblyReloadEvents.beforeAssemblyReload += StopServer;
            EditorApplication.quitting += StopServer;
        }

        static void StartServer()
        {
            if (server != null)
                return;

            UnityToolRegistry registry = new(new IUnityEditorTool[]
            {
                new GetSelectionTool(),
                new GetSceneHierarchyTool(),
                new GetGameObjectDetailsTool(),
                new CreateGameObjectTool(),
                new CreatePrimitiveTool(),
                new SetTransformTool(),
                new DeleteGameObjectTool(),
                new SetGameObjectActiveTool(),
                new FindGameObjectsTool(),
                new GetComponentsTool(),
                new AddComponentTool(),
                new SetComponentPropertyTool(),
                new CreateMaterialTool(),
                new AssignMaterialTool(),
                new SetMaterialColorTool(),
                new GetSceneInfoTool(),
                new SaveSceneTool(),
                new ListAssetsTool(),
                new SetGameObjectNameTool(),
                new SetGameObjectParentTool(),
                new DuplicateGameObjectTool(),
                new CreateFolderTool(),
                new CreateCanvasTool(),
                new CreateUIElementTool(),
                new SetRectTransformTool(),
                new SetUITextTool(),
                new SetUIColorTool(),
                new ExecuteEditorMenuTool(),
                new SavePrefabTool(),
                new SetGameObjectTagTool(),
                new SetSerializedFieldTool(),
            });

            server = new ToryBridgeServer(registry, ToryBridgeServer.DefaultPort);
            server.Start();
        }

        static void StopServer()
        {
            server?.Dispose();
            server = null;
        }
    }
}