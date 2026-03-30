using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetGameObjectTagTool : IUnityEditorTool
    {
        public string Name => "set_gameobject_tag";
        public string Description => "Sets the Tag on a GameObject. The tag must already exist in the project.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"GameObject InstanceID\"},\"tag\":{\"type\":\"string\",\"description\":\"Tag string to assign (must exist in project)\"}},\"required\":[\"instanceId\",\"tag\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null ||
                    !args.TryGetValue("instanceId", out var idVal) ||
                    !args.TryGetValue("tag", out var tagVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId and tag are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                string newTag = tagVal.ToString();
                string oldTag = go.tag;

                Undo.RecordObject(go, "Set GameObject Tag");
                go.tag = newTag;
                EditorUtility.SetDirty(go);

                return JsonConvert.SerializeObject(new { success = true, oldTag = oldTag, newTag = go.tag });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
