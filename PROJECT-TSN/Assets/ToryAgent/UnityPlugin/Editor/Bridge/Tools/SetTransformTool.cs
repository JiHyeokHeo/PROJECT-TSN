using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetTransformTool : IUnityEditorTool
    {
        public string Name => "set_transform";
        public string Description => "Sets the position, rotation, and/or scale of a GameObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the GameObject\"},\"position\":{\"type\":\"object\",\"description\":\"{x,y,z}\"},\"rotation\":{\"type\":\"object\",\"description\":\"Euler angles {x,y,z}\"},\"scale\":{\"type\":\"object\",\"description\":\"{x,y,z}\"},\"useLocalSpace\":{\"type\":\"boolean\",\"description\":\"Use local space (default: true)\"}},\"required\":[\"instanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null) return JsonConvert.SerializeObject(new { error = "Invalid arguments" });

                if (!args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                Undo.RecordObject(go.transform, "Set Transform");

                bool local = true;
                if (args.TryGetValue("useLocalSpace", out var ls))
                    local = Convert.ToBoolean(ls);

                if (args.TryGetValue("position", out var posVal))
                {
                    var pos = JsonConvert.DeserializeObject<Dictionary<string, float>>(posVal.ToString());
                    if (pos != null)
                    {
                        var v = new Vector3(
                            pos.TryGetValue("x", out var px) ? px : (local ? go.transform.localPosition.x : go.transform.position.x),
                            pos.TryGetValue("y", out var py) ? py : (local ? go.transform.localPosition.y : go.transform.position.y),
                            pos.TryGetValue("z", out var pz) ? pz : (local ? go.transform.localPosition.z : go.transform.position.z));
                        if (local) go.transform.localPosition = v;
                        else go.transform.position = v;
                    }
                }

                if (args.TryGetValue("rotation", out var rotVal))
                {
                    var rot = JsonConvert.DeserializeObject<Dictionary<string, float>>(rotVal.ToString());
                    if (rot != null)
                    {
                        var v = new Vector3(
                            rot.TryGetValue("x", out var rx) ? rx : (local ? go.transform.localEulerAngles.x : go.transform.eulerAngles.x),
                            rot.TryGetValue("y", out var ry) ? ry : (local ? go.transform.localEulerAngles.y : go.transform.eulerAngles.y),
                            rot.TryGetValue("z", out var rz) ? rz : (local ? go.transform.localEulerAngles.z : go.transform.eulerAngles.z));
                        if (local) go.transform.localEulerAngles = v;
                        else go.transform.eulerAngles = v;
                    }
                }

                if (args.TryGetValue("scale", out var scaleVal))
                {
                    var sc = JsonConvert.DeserializeObject<Dictionary<string, float>>(scaleVal.ToString());
                    if (sc != null)
                    {
                        go.transform.localScale = new Vector3(
                            sc.TryGetValue("x", out var sx) ? sx : go.transform.localScale.x,
                            sc.TryGetValue("y", out var sy) ? sy : go.transform.localScale.y,
                            sc.TryGetValue("z", out var sz) ? sz : go.transform.localScale.z);
                    }
                }

                var lp = go.transform.localPosition;
                var le = go.transform.localEulerAngles;
                var lsc = go.transform.localScale;

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    localPosition = new { x = lp.x, y = lp.y, z = lp.z },
                    localRotation = new { x = le.x, y = le.y, z = le.z },
                    localScale = new { x = lsc.x, y = lsc.y, z = lsc.z }
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
