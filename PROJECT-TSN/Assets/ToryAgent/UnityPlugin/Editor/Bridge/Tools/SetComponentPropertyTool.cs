using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetComponentPropertyTool : IUnityEditorTool
    {
        public string Name => "set_component_property";
        public string Description => "Sets a property or field value on a specific component of a GameObject using reflection.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"GameObject InstanceID\"},\"componentType\":{\"type\":\"string\",\"description\":\"Component type name e.g. Light, Camera, Rigidbody\"},\"propertyName\":{\"type\":\"string\",\"description\":\"Property or field name\"},\"value\":{\"description\":\"Value to set (string, number, or bool)\"}},\"required\":[\"instanceId\",\"componentType\",\"propertyName\",\"value\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null ||
                    !args.TryGetValue("instanceId", out var idVal) ||
                    !args.TryGetValue("componentType", out var typeVal) ||
                    !args.TryGetValue("propertyName", out var propVal) ||
                    !args.TryGetValue("value", out var valVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId, componentType, propertyName, and value are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                string typeName = typeVal.ToString();
                Component comp = go.GetComponent(typeName);
                if (comp == null)
                {
                    Type t = Type.GetType($"UnityEngine.{typeName}, UnityEngine")
                        ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.CoreModule");
                    if (t != null) comp = go.GetComponent(t);
                }
                if (comp == null) return JsonConvert.SerializeObject(new { error = $"Component {typeName} not found" });

                string propName = propVal.ToString();
                var compType = comp.GetType();

                Undo.RecordObject(comp, $"Set {typeName}.{propName}");

                const BindingFlags allInstance =
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                // ── helper: resolve value for a given target type ────────────────
                object ResolveValue(Type targetType, object raw)
                {
                    // Unity Object reference: caller passes an instanceId (integer)
                    if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                    {
                        int refId = Convert.ToInt32(raw.ToString());
                        var unityObj = EditorUtility.EntityIdToObject(refId) as UnityEngine.Object;
                        if (unityObj == null)
                            throw new InvalidOperationException($"Unity Object with instanceId {refId} not found.");
                        return unityObj;
                    }
                    return Convert.ChangeType(raw.ToString(), targetType);
                }

                // Try property first (public)
                var prop = compType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(comp, ResolveValue(prop.PropertyType, valVal));
                    EditorUtility.SetDirty(comp);
                    return JsonConvert.SerializeObject(new { success = true, property = propName, value = valVal.ToString() });
                }

                // Try field (public and private, walk up hierarchy)
                FieldInfo field = null;
                for (var t = compType; t != null && field == null; t = t.BaseType)
                    field = t.GetField(propName, allInstance);

                if (field != null)
                {
                    field.SetValue(comp, ResolveValue(field.FieldType, valVal));
                    EditorUtility.SetDirty(comp);
                    return JsonConvert.SerializeObject(new { success = true, field = propName, value = valVal.ToString() });
                }

                return JsonConvert.SerializeObject(new { error = $"Property/field '{propName}' not found on {typeName}" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
