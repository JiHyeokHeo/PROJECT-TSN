using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class AddComponentTool : IUnityEditorTool
    {
        public string Name => "add_component";
        public string Description => "Adds a component to a GameObject. Use the component type name (e.g. 'Rigidbody', 'BoxCollider', 'Light', 'Camera', 'AudioSource').";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"componentType\":{\"type\":\"string\",\"description\":\"Component type name e.g. Rigidbody, BoxCollider, Light\"}},\"required\":[\"instanceId\",\"componentType\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (!args.TryGetValue("instanceId", out var idVal) || !args.TryGetValue("componentType", out var typeVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId and componentType are required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null) return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                string typeName = typeVal.ToString();
                Type compType = Type.GetType(typeName)
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine")
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.CoreModule")
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.PhysicsModule")
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.AudioModule")
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.UIModule")
                    ?? Type.GetType($"UnityEngine.{typeName}, UnityEngine.AnimationModule");

                // 사용자 스크립트 어셈블리(Assembly-CSharp 등) 탐색
                if (compType == null)
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        // 단순 클래스 이름으로 탐색
                        compType = asm.GetType(typeName, throwOnError: false, ignoreCase: true);
                        if (compType != null) break;

                        // "TST.ClassName" 형식도 시도
                        if (!typeName.Contains('.'))
                        {
                            compType = asm.GetType($"TST.{typeName}", throwOnError: false, ignoreCase: true);
                            if (compType != null) break;
                        }
                    }
                }

                if (compType == null)
                    return JsonConvert.SerializeObject(new { error = $"Component type not found: {typeName}" });

                var comp = Undo.AddComponent(go, compType);
                if (comp == null)
                    return JsonConvert.SerializeObject(new { error = $"Failed to add component: {typeName}" });

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    typeName = comp.GetType().Name,
                    instanceId = comp.GetInstanceID()
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
