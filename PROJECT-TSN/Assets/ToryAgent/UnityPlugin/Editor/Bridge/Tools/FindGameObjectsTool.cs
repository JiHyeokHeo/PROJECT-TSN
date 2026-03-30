using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class FindGameObjectsTool : IUnityEditorTool
    {
        public string Name => "find_gameobjects";
        public string Description => "Finds GameObjects by name or tag in the current scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Partial or full name to search\"},\"tag\":{\"type\":\"string\",\"description\":\"Tag to filter by\"},\"includeInactive\":{\"type\":\"boolean\",\"description\":\"Include inactive objects (default: true)\"}},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                bool includeInactive = true;
                if (args != null && args.TryGetValue("includeInactive", out var ia))
                    includeInactive = Convert.ToBoolean(ia);

                string nameFilter = args != null && args.TryGetValue("name", out var nv) ? nv?.ToString() : null;
                string tagFilter = args != null && args.TryGetValue("tag", out var tv) ? tv?.ToString() : null;

                var all = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var results = new List<object>();

                foreach (var go in all)
                {
                    if (go == null) continue;
                    if (go.hideFlags != HideFlags.None) continue;
                    if (!includeInactive && !go.activeSelf) continue;
                    if (nameFilter != null && !go.name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) continue;
                    if (tagFilter != null && go.tag != tagFilter) continue;

                    results.Add(new
                    {
                        instanceId = go.GetInstanceID(),
                        name = go.name,
                        hierarchyPath = GetPath(go.transform),
                        isActive = go.activeSelf,
                        tag = go.tag
                    });
                }

                return JsonConvert.SerializeObject(new { count = results.Count, objects = results });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }

        static string GetPath(Transform t)
        {
            string path = t.name;
            Transform current = t.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
