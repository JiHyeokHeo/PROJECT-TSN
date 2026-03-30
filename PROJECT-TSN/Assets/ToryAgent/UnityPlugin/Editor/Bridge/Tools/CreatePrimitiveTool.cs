using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreatePrimitiveTool : IUnityEditorTool
    {
        public string Name => "create_primitive";
        public string Description => "Creates a Unity primitive GameObject (Cube, Sphere, Capsule, Cylinder, Plane, Quad) in the scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"primitiveType\":{\"type\":\"string\",\"enum\":[\"Cube\",\"Sphere\",\"Capsule\",\"Cylinder\",\"Plane\",\"Quad\"],\"description\":\"Type of primitive\"},\"name\":{\"type\":\"string\"},\"position\":{\"type\":\"object\",\"description\":\"{x,y,z}\"},\"scale\":{\"type\":\"object\",\"description\":\"{x,y,z}\"},\"parentInstanceId\":{\"type\":\"number\"}},\"required\":[\"primitiveType\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null) return JsonConvert.SerializeObject(new { error = "Invalid arguments" });

                if (!args.TryGetValue("primitiveType", out var ptVal))
                    return JsonConvert.SerializeObject(new { error = "primitiveType is required" });

                if (!Enum.TryParse<PrimitiveType>(ptVal.ToString(), out var primitiveType))
                    return JsonConvert.SerializeObject(new { error = $"Unknown primitiveType: {ptVal}" });

                var go = GameObject.CreatePrimitive(primitiveType);
                Undo.RegisterCreatedObjectUndo(go, $"Create {primitiveType}");

                if (args.TryGetValue("name", out var nameVal))
                    go.name = nameVal.ToString();

                if (args.TryGetValue("parentInstanceId", out var pid))
                {
                    var parent = EditorUtility.EntityIdToObject(Convert.ToInt32(pid)) as GameObject;
                    if (parent != null) go.transform.SetParent(parent.transform, false);
                }

                if (args.TryGetValue("position", out var posVal))
                {
                    var pos = JsonConvert.DeserializeObject<Dictionary<string, float>>(posVal.ToString());
                    if (pos != null)
                        go.transform.position = new Vector3(
                            pos.TryGetValue("x", out var px) ? px : 0,
                            pos.TryGetValue("y", out var py) ? py : 0,
                            pos.TryGetValue("z", out var pz) ? pz : 0);
                }

                if (args.TryGetValue("scale", out var scaleVal))
                {
                    var sc = JsonConvert.DeserializeObject<Dictionary<string, float>>(scaleVal.ToString());
                    if (sc != null)
                        go.transform.localScale = new Vector3(
                            sc.TryGetValue("x", out var sx) ? sx : 1,
                            sc.TryGetValue("y", out var sy) ? sy : 1,
                            sc.TryGetValue("z", out var sz) ? sz : 1);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    instanceId = go.GetInstanceID(),
                    name = go.name
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
