using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetUITextTool : IUnityEditorTool
    {
        public string Name => "set_ui_text";
        public string Description => "Sets text content, fontSize, and color on a UI Text or TMP_Text component.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"text\":{\"type\":\"string\"},\"fontSize\":{\"type\":\"number\"},\"color\":{\"type\":\"object\",\"description\":\"{r,g,b,a}\"}},\"required\":[\"instanceId\",\"text\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });
                if (!args.TryGetValue("text", out var textVal))
                    return JsonConvert.SerializeObject(new { error = "text is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null)
                    return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                // Try TMP_Text first via reflection (avoids hard assembly dependency)
                var tmpType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
                if (tmpType != null)
                {
                    var tmp = go.GetComponent(tmpType);
                    if (tmp != null)
                    {
                        Undo.RecordObject(tmp, "Set TMP Text");
                        tmpType.GetProperty("text")?.SetValue(tmp, textVal.ToString());
                        if (args.TryGetValue("fontSize", out var fs))
                            tmpType.GetProperty("fontSize")?.SetValue(tmp, Convert.ToSingle(fs));
                        if (args.TryGetValue("color", out var col))
                            SetColor(tmpType.GetProperty("color"), tmp, col.ToString());
                        return JsonConvert.SerializeObject(new { success = true, componentType = "TMP_Text", instanceId = go.GetInstanceID() });
                    }
                }

                // Fallback: legacy UnityEngine.UI.Text
                var uiText = go.GetComponent<Text>();
                if (uiText != null)
                {
                    Undo.RecordObject(uiText, "Set UI Text");
                    uiText.text = textVal.ToString();
                    if (args.TryGetValue("fontSize", out var fs))
                        uiText.fontSize = Convert.ToInt32(fs);
                    if (args.TryGetValue("color", out var col))
                    {
                        var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(col.ToString());
                        if (d != null)
                            uiText.color = new Color(
                                d.TryGetValue("r", out var r) ? r : 1f,
                                d.TryGetValue("g", out var g) ? g : 1f,
                                d.TryGetValue("b", out var b) ? b : 1f,
                                d.TryGetValue("a", out var a) ? a : 1f);
                    }
                    return JsonConvert.SerializeObject(new { success = true, componentType = "Text", instanceId = go.GetInstanceID() });
                }

                return JsonConvert.SerializeObject(new { error = "No Text or TMP_Text component found on GameObject" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }

        private static void SetColor(System.Reflection.PropertyInfo prop, object target, string colorJson)
        {
            if (prop == null) return;
            var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(colorJson);
            if (d == null) return;
            prop.SetValue(target, new Color(
                d.TryGetValue("r", out var r) ? r : 1f,
                d.TryGetValue("g", out var g) ? g : 1f,
                d.TryGetValue("b", out var b) ? b : 1f,
                d.TryGetValue("a", out var a) ? a : 1f));
        }
    }
}
