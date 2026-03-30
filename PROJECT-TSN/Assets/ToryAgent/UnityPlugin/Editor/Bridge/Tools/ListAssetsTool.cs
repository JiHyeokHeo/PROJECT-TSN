using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class ListAssetsTool : IUnityEditorTool
    {
        public string Name => "list_assets";
        public string Description => "Lists assets in a given folder path. Use filter like 't:Material', 't:Texture2D', 't:Prefab', 't:Scene'.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"path\":{\"type\":\"string\",\"description\":\"Asset folder path e.g. Assets/Materials\"},\"filter\":{\"type\":\"string\",\"description\":\"Search filter e.g. t:Material\"},\"recursive\":{\"type\":\"boolean\",\"description\":\"Include subfolders (default: true)\"}},\"required\":[\"path\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("path", out var pathVal))
                    return JsonConvert.SerializeObject(new { error = "path is required" });

                string path = pathVal.ToString();
                string filter = args.TryGetValue("filter", out var fv) ? fv.ToString() : "";
                bool recursive = !args.TryGetValue("recursive", out var rv) || Convert.ToBoolean(rv);

                var guids = AssetDatabase.FindAssets(filter, new[] { path });

                var assets = new List<object>();
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!recursive && System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/') != path.TrimEnd('/'))
                        continue;

                    var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                    assets.Add(new
                    {
                        assetPath = assetPath,
                        name = System.IO.Path.GetFileNameWithoutExtension(assetPath),
                        type = type?.Name ?? "Unknown",
                        guid = guid
                    });
                }

                return JsonConvert.SerializeObject(new { count = assets.Count, assets = assets });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
