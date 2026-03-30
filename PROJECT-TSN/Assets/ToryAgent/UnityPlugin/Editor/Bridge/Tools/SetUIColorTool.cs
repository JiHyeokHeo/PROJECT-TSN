using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetUIColorTool : IUnityEditorTool
    {
        public string Name => "set_ui_color";
        public string Description => "Sets the color on a UI Graphic component (Image, Text, RawImage, etc.).";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"color\":{\"type\":\"object\",\"description\":\"{r,g,b,a} values 0-1\"}},\"required\":[\"instanceId\",\"color\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });
                if (!args.TryGetValue("color", out var colVal))
                    return JsonConvert.SerializeObject(new { error = "color is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null)
                    return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var graphic = go.GetComponent<Graphic>();
                if (graphic == null)
                    return JsonConvert.SerializeObject(new { error = "No Graphic component found on GameObject" });

                var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(colVal.ToString());
                if (d == null)
                    return JsonConvert.SerializeObject(new { error = "Invalid color format" });

                Undo.RecordObject(graphic, "Set UI Color");
                graphic.color = new Color(
                    d.TryGetValue("r", out var r) ? r : 1f,
                    d.TryGetValue("g", out var g) ? g : 1f,
                    d.TryGetValue("b", out var b) ? b : 1f,
                    d.TryGetValue("a", out var a) ? a : 1f);

                return JsonConvert.SerializeObject(new
                {
                    success       = true,
                    instanceId    = go.GetInstanceID(),
                    componentType = graphic.GetType().Name
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
