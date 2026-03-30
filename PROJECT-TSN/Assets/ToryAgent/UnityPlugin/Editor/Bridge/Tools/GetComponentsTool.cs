using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class GetComponentsTool : IUnityEditorTool
    {
        public string Name => "get_components";
        public string Description => "Returns all components on a GameObject with their key properties.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the GameObject\"}},\"required\":[\"instanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var components = new List<object>();
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    var props = new Dictionary<string, string>();
                    var type = comp.GetType();

                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        try { props[field.Name] = field.GetValue(comp)?.ToString() ?? "null"; } catch { }
                    }
                    foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
                        try { props[prop.Name] = prop.GetValue(comp)?.ToString() ?? "null"; } catch { }
                    }

                    components.Add(new
                    {
                        typeName = type.Name,
                        fullTypeName = type.FullName,
                        instanceId = comp.GetInstanceID(),
                        properties = props
                    });
                }

                return JsonConvert.SerializeObject(new
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    componentCount = components.Count,
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
