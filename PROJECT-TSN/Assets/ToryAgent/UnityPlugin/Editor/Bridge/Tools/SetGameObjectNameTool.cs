using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetGameObjectNameTool : IUnityEditorTool
    {
        public string Name => "set_gameobject_name";
        public string Description => "Renames a GameObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"name\":{\"type\":\"string\"}},\"required\":[\"instanceId\",\"name\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal) || !args.TryGetValue("name", out var nameVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId and name are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                string oldName = go.name;
                Undo.RecordObject(go, "Rename GameObject");
                go.name = nameVal.ToString();

                return JsonConvert.SerializeObject(new { success = true, oldName = oldName, newName = go.name });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
