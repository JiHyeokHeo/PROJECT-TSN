using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetGameObjectActiveTool : IUnityEditorTool
    {
        public string Name => "set_gameobject_active";
        public string Description => "Sets the active state of a GameObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"active\":{\"type\":\"boolean\"}},\"required\":[\"instanceId\",\"active\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                bool active = args.TryGetValue("active", out var av) && Convert.ToBoolean(av);

                Undo.RecordObject(go, "Set Active");
                go.SetActive(active);

                return JsonConvert.SerializeObject(new { success = true, name = go.name, active = go.activeSelf });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
