using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreateGameObjectTool : IUnityEditorTool
    {
        public string Name => "create_gameobject";
        public string Description => "Creates a new empty GameObject in the scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Name of the new GameObject\"},\"parentInstanceId\":{\"type\":\"number\",\"description\":\"InstanceID of parent GameObject (optional)\"},\"position\":{\"type\":\"object\",\"description\":\"World position {x,y,z}\"},\"rotation\":{\"type\":\"object\",\"description\":\"Euler rotation {x,y,z}\"},\"tag\":{\"type\":\"string\"},\"layer\":{\"type\":\"number\"}},\"required\":[\"name\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null) return JsonConvert.SerializeObject(new { error = "Invalid arguments" });

                string goName = args.TryGetValue("name", out var n) ? n.ToString() : "New GameObject";

                var go = new GameObject(goName);
                Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");

                if (args.TryGetValue("parentInstanceId", out var pid))
                {
                    int parentId = Convert.ToInt32(pid);
                    var parent = EditorUtility.EntityIdToObject(parentId) as GameObject;
                    if (parent != null)
                        go.transform.SetParent(parent.transform, false);
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

                if (args.TryGetValue("rotation", out var rotVal))
                {
                    var rot = JsonConvert.DeserializeObject<Dictionary<string, float>>(rotVal.ToString());
                    if (rot != null)
                        go.transform.eulerAngles = new Vector3(
                            rot.TryGetValue("x", out var rx) ? rx : 0,
                            rot.TryGetValue("y", out var ry) ? ry : 0,
                            rot.TryGetValue("z", out var rz) ? rz : 0);
                }

                if (args.TryGetValue("tag", out var tagVal))
                    try { go.tag = tagVal.ToString(); } catch { }

                if (args.TryGetValue("layer", out var layerVal))
                    go.layer = Convert.ToInt32(layerVal);

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
