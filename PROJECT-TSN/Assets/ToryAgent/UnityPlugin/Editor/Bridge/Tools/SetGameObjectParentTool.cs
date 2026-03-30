using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetGameObjectParentTool : IUnityEditorTool
    {
        public string Name => "set_gameobject_parent";
        public string Description => "Changes the parent of a GameObject. Set parentInstanceId to null/0 to move to scene root.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"parentInstanceId\":{\"type\":\"number\",\"description\":\"0 or omit to unparent to root\"},\"worldPositionStays\":{\"type\":\"boolean\",\"description\":\"Keep world position (default: true)\"}},\"required\":[\"instanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                bool worldStays = !args.TryGetValue("worldPositionStays", out var wsv) || Convert.ToBoolean(wsv);

                Transform newParent = null;
                if (args.TryGetValue("parentInstanceId", out var pidVal))
                {
                    int pid = Convert.ToInt32(pidVal);
                    if (pid != 0)
                    {
                        var parentGo = EditorUtility.EntityIdToObject(pid) as GameObject;
                        if (parentGo == null) return JsonConvert.SerializeObject(new { error = "Parent GameObject not found" });
                        newParent = parentGo.transform;
                    }
                }

                Undo.SetTransformParent(go.transform, newParent, worldStays, "Set Parent");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    name = go.name,
                    newParent = newParent != null ? newParent.name : "(root)"
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
