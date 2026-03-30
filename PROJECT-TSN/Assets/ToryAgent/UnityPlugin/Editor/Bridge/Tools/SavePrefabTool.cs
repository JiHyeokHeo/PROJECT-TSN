using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ToryAgent.UnityPlugin.Editor
{
    /// <summary>
    /// 씬의 GameObject를 프리팹 에셋으로 저장합니다.
    /// PrefabUtility.SaveAsPrefabAsset을 사용하므로 원본 씬 오브젝트는 그대로 유지됩니다.
    /// </summary>
    public sealed class SavePrefabTool : IUnityEditorTool
    {
        public string Name => "save_prefab";

        public string Description =>
            "Saves a scene GameObject as a Prefab asset at the specified path. " +
            "Creates any missing intermediate folders automatically. " +
            "The original scene object remains intact. " +
            "Call AssetDatabase.SaveAssets after bulk operations.";

        public string InputSchemaJson =>
            "{\"type\":\"object\"," +
            "\"properties\":{" +
            "\"instanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the root GameObject to save as prefab\"}," +
            "\"assetPath\":{\"type\":\"string\",\"description\":\"Project-relative path, e.g. Assets/Prefabs/MyPrefab.prefab\"}," +
            "\"destroySceneObject\":{\"type\":\"boolean\",\"description\":\"If true, destroy the scene object after saving (default: false)\"}" +
            "}," +
            "\"required\":[\"instanceId\",\"assetPath\"]," +
            "\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null)
                    return JsonConvert.SerializeObject(new { error = "Invalid arguments" });

                if (!args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                if (!args.TryGetValue("assetPath", out var pathVal) || string.IsNullOrWhiteSpace(pathVal?.ToString()))
                    return JsonConvert.SerializeObject(new { error = "assetPath is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null)
                    return JsonConvert.SerializeObject(new { error = $"GameObject not found for instanceId {idVal}" });

                string assetPath = pathVal.ToString();

                // 확장자 보정
                if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    assetPath += ".prefab";

                // 중간 폴더 자동 생성
                EnsureFolders(assetPath);

                var prefab = PrefabUtility.SaveAsPrefabAsset(go, assetPath, out bool success);

                if (!success || prefab == null)
                    return JsonConvert.SerializeObject(new { error = $"PrefabUtility.SaveAsPrefabAsset failed for path: {assetPath}" });

                // destroySceneObject 옵션
                bool destroy = args.TryGetValue("destroySceneObject", out var dv) && Convert.ToBoolean(dv);
                if (destroy)
                    UnityEngine.Object.DestroyImmediate(go);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    assetPath,
                    guid = AssetDatabase.AssetPathToGUID(assetPath)
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }

        // "Assets/A/B/C/Prefab.prefab" -> 각 단계별 폴더를 AssetDatabase.CreateFolder로 생성
        static void EnsureFolders(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(dir)) return;

            string[] parts = dir.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
