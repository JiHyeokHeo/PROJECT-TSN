using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class GetSceneHierarchyTool : IUnityEditorTool
    {
        public string Name => "get_scene_hierarchy";
        public string Description => "Returns the full GameObject hierarchy of the current Unity scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"maxDepth\":{\"type\":\"number\",\"description\":\"Max depth to traverse (default: 10)\"}},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                int maxDepth = 10;
                if (!string.IsNullOrEmpty(argumentsJson) && argumentsJson != "{}")
                {
                    var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson);
                    if (args != null && args.TryGetValue("maxDepth", out var d))
                        maxDepth = Convert.ToInt32(d);
                }

                var scene = SceneManager.GetActiveScene();
                var roots = scene.GetRootGameObjects();
                var nodes = new List<object>();
                foreach (var root in roots)
                    nodes.Add(BuildNode(root, 0, maxDepth));

                return JsonConvert.SerializeObject(new
                {
                    sceneName = scene.name,
                    scenePath = scene.path,
                    rootCount = roots.Length,
                    hierarchy = nodes
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }

        static object BuildNode(GameObject go, int depth, int maxDepth)
        {
            var t = go.transform;
            var compNames = new List<string>();
            foreach (var c in go.GetComponents<Component>())
                if (c != null) compNames.Add(c.GetType().Name);

            var children = new List<object>();
            if (depth < maxDepth)
            {
                for (int i = 0; i < t.childCount; i++)
                    children.Add(BuildNode(t.GetChild(i).gameObject, depth + 1, maxDepth));
            }

            var lp = t.localPosition;
            var lr = t.localEulerAngles;
            var ls = t.localScale;

            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                isActive = go.activeSelf,
                tag = go.tag,
                layer = go.layer,
                layerName = LayerMask.LayerToName(go.layer),
                childCount = t.childCount,
                components = compNames,
                localPosition = new { x = lp.x, y = lp.y, z = lp.z },
                localRotation = new { x = lr.x, y = lr.y, z = lr.z },
                localScale = new { x = ls.x, y = ls.y, z = ls.z },
                children = children
            };
        }
    }
}
