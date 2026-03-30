using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetSerializedFieldTool : IUnityEditorTool
    {
        public string Name => "set_serialized_field";
        public string Description => "Sets an object reference on a SerializeField (including private fields) via SerializedObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the target GameObject\"},\"componentType\":{\"type\":\"string\",\"description\":\"Component type name e.g. MyScript\"},\"fieldName\":{\"type\":\"string\",\"description\":\"Serialized field name\"},\"targetInstanceId\":{\"type\":\"number\",\"description\":\"InstanceID of the object to assign as the reference\"}},\"required\":[\"instanceId\",\"componentType\",\"fieldName\",\"targetInstanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null ||
                    !args.TryGetValue("instanceId", out var idVal) ||
                    !args.TryGetValue("componentType", out var typeVal) ||
                    !args.TryGetValue("fieldName", out var fieldNameVal) ||
                    !args.TryGetValue("targetInstanceId", out var targetIdVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId, componentType, fieldName, and targetInstanceId are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                string typeName = typeVal.ToString();
                Component comp = go.GetComponent(typeName);
                if (comp == null)
                    return JsonConvert.SerializeObject(new { error = $"Component {typeName} not found on GameObject" });

                var so = new SerializedObject(comp);
                string fieldName = fieldNameVal.ToString();
                var prop = so.FindProperty(fieldName);
                if (prop == null)
                    return JsonConvert.SerializeObject(new { error = $"Serialized field '{fieldName}' not found on {typeName}" });

                var targetObj = EditorUtility.EntityIdToObject(Convert.ToInt32(targetIdVal));
                if (targetObj == null)
                    return JsonConvert.SerializeObject(new { error = $"Target object with instanceId {targetIdVal} not found" });

                prop.objectReferenceValue = targetObj;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(comp);

                return JsonConvert.SerializeObject(new { success = true, fieldName = fieldName, assignedObject = targetObj.name });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
