using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class GetSelectionTool : IUnityEditorTool
    {
        public string Name => "get_selection";

        public string Description => "Returns information about the current Unity selection.";

        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            Object active = Selection.activeObject;
            GameObject activeGameObject = Selection.activeGameObject;

            var result = new
            {
                hasSelection = active != null,
                name = active != null ? active.name : "",
                objectType = active != null ? active.GetType().Name : "",
                assetPath = active != null ? AssetDatabase.GetAssetPath(active) : "",
                instanceId = active != null ? active.GetInstanceID() : 0,
                isGameObject = activeGameObject != null,
                scenePath = activeGameObject != null && activeGameObject.scene.IsValid()
                    ? activeGameObject.scene.path
                    : "",
                hierarchyPath = activeGameObject != null
                    ? GetHierarchyPath(activeGameObject.transform)
                    : "",
            };

            return JsonConvert.SerializeObject(result);
        }

        static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = transform.name;
            Transform current = transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}