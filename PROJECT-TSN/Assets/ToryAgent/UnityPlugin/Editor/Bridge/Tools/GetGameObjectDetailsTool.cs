using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class GetGameObjectDetailsTool : IUnityEditorTool
    {
        public string Name => "get_gameobject_details";
        public string Description => "Returns detailed information about a specific GameObject including all components and transform data.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the GameObject\"},\"path\":{\"type\":\"string\",\"description\":\"Hierarchy path like Root/Child/Target\"}},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                GameObject go = null;

                if (args != null && args.TryGetValue("instanceId", out var idVal))
                {
                    int id = Convert.ToInt32(idVal);
                    go = EditorUtility.EntityIdToObject(id) as GameObject;
                }
                else if (args != null && args.TryGetValue("path", out var pathVal))
                {
                    go = GameObject.Find(pathVal.ToString());
                }

                if (go == null)
                    return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var t = go.transform;
                var wp = t.position;
                var wr = t.eulerAngles;
                var lp = t.localPosition;
                var lr = t.localEulerAngles;
                var ls = t.localScale;

                var components = new List<object>();
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    var props = new Dictionary<string, object>();
                    try
                    {
                        var type = comp.GetType();
                        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (!prop.CanRead) continue;
                            if (prop.GetIndexParameters().Length > 0) continue;
                            try
                            {
                                var val = prop.GetValue(comp);
                                if (val == null) props[prop.Name] = null;
                                else if (val is Vector3 v3) props[prop.Name] = new { x = v3.x, y = v3.y, z = v3.z };
                                else if (val is Color col) props[prop.Name] = new { r = col.r, g = col.g, b = col.b, a = col.a };
                                else if (val is bool || val is int || val is float || val is double || val is string || val.GetType().IsEnum)
                                    props[prop.Name] = val.ToString();
                            }
                            catch { }
                        }
                    }
                    catch { }

                    components.Add(new { typeName = comp.GetType().Name, properties = props });
                }

                return JsonConvert.SerializeObject(new
                {
                    name = go.name,
                    instanceId = go.GetInstanceID(),
                    tag = go.tag,
                    layer = go.layer,
                    layerName = LayerMask.LayerToName(go.layer),
                    isActive = go.activeSelf,
                    isActiveInHierarchy = go.activeInHierarchy,
                    assetPath = AssetDatabase.GetAssetPath(go),
                    worldPosition = new { x = wp.x, y = wp.y, z = wp.z },
                    worldRotation = new { x = wr.x, y = wr.y, z = wr.z },
                    localPosition = new { x = lp.x, y = lp.y, z = lp.z },
                    localRotation = new { x = lr.x, y = lr.y, z = lr.z },
                    localScale = new { x = ls.x, y = ls.y, z = ls.z },
                    components = components
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
