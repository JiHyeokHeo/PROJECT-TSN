using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class AssignMaterialTool : IUnityEditorTool
    {
        public string Name => "assign_material";
        public string Description => "Assigns a material asset to a Renderer component on a GameObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"GameObject InstanceID\"},\"materialPath\":{\"type\":\"string\",\"description\":\"Asset path to the material e.g. Assets/Materials/MyMat.mat\"},\"materialIndex\":{\"type\":\"number\",\"description\":\"Material slot index (default: 0)\"}},\"required\":[\"instanceId\",\"materialPath\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal) || !args.TryGetValue("materialPath", out var pathVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId and materialPath are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var renderer = go.GetComponent<Renderer>();
                if (renderer == null) return JsonConvert.SerializeObject(new { error = "No Renderer found on GameObject" });

                var mat = AssetDatabase.LoadAssetAtPath<Material>(pathVal.ToString());
                if (mat == null) return JsonConvert.SerializeObject(new { error = $"Material not found at: {pathVal}" });

                int idx = args.TryGetValue("materialIndex", out var idxVal) ? Convert.ToInt32(idxVal) : 0;

                Undo.RecordObject(renderer, "Assign Material");
                var mats = renderer.sharedMaterials;
                if (idx >= mats.Length)
                    return JsonConvert.SerializeObject(new { error = $"Material index {idx} out of range (count: {mats.Length})" });

                mats[idx] = mat;
                renderer.sharedMaterials = mats;
                EditorUtility.SetDirty(renderer);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    gameObject = go.name,
                    materialName = mat.name,
                    slotIndex = idx
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
