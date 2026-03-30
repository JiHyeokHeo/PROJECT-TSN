using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class DuplicateGameObjectTool : IUnityEditorTool
    {
        public string Name => "duplicate_gameobject";
        public string Description => "Duplicates a GameObject (supports Undo).";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"}},\"required\":[\"instanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var copy = UnityEngine.Object.Instantiate(go, go.transform.parent);
                copy.name = go.name + " (Copy)";
                Undo.RegisterCreatedObjectUndo(copy, $"Duplicate {go.name}");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    newInstanceId = copy.GetInstanceID(),
                    name = copy.name
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
