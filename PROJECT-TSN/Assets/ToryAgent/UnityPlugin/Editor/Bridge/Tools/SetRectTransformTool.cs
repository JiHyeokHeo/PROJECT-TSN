using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetRectTransformTool : IUnityEditorTool
    {
        public string Name => "set_rect_transform";
        public string Description => "Sets RectTransform properties (anchoredPosition, sizeDelta, anchorMin, anchorMax, pivot) on a UI GameObject.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"instanceId\":{\"type\":\"number\"},\"anchoredPosition\":{\"type\":\"object\",\"description\":\"{x,y}\"},\"sizeDelta\":{\"type\":\"object\",\"description\":\"{x,y}\"},\"anchorMin\":{\"type\":\"object\",\"description\":\"{x,y}\"},\"anchorMax\":{\"type\":\"object\",\"description\":\"{x,y}\"},\"pivot\":{\"type\":\"object\",\"description\":\"{x,y}\"}},\"required\":[\"instanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("instanceId", out var idVal))
                    return JsonConvert.SerializeObject(new { error = "instanceId is required" });

                var go = EditorUtility.EntityIdToObject(Convert.ToInt32(idVal)) as GameObject;
                if (go == null)
                    return JsonConvert.SerializeObject(new { error = "GameObject not found" });

                var rt = go.GetComponent<RectTransform>();
                if (rt == null)
                    return JsonConvert.SerializeObject(new { error = "GameObject has no RectTransform" });

                Undo.RecordObject(rt, "Set RectTransform");

                if (args.TryGetValue("anchoredPosition", out var apVal))
                {
                    var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(apVal.ToString());
                    if (d != null)
                        rt.anchoredPosition = new Vector2(
                            d.TryGetValue("x", out var x) ? x : rt.anchoredPosition.x,
                            d.TryGetValue("y", out var y) ? y : rt.anchoredPosition.y);
                }

                if (args.TryGetValue("sizeDelta", out var sdVal))
                {
                    var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(sdVal.ToString());
                    if (d != null)
                        rt.sizeDelta = new Vector2(
                            d.TryGetValue("x", out var x) ? x : rt.sizeDelta.x,
                            d.TryGetValue("y", out var y) ? y : rt.sizeDelta.y);
                }

                if (args.TryGetValue("anchorMin", out var aminVal))
                {
                    var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(aminVal.ToString());
                    if (d != null)
                        rt.anchorMin = new Vector2(
                            d.TryGetValue("x", out var x) ? x : rt.anchorMin.x,
                            d.TryGetValue("y", out var y) ? y : rt.anchorMin.y);
                }

                if (args.TryGetValue("anchorMax", out var amaxVal))
                {
                    var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(amaxVal.ToString());
                    if (d != null)
                        rt.anchorMax = new Vector2(
                            d.TryGetValue("x", out var x) ? x : rt.anchorMax.x,
                            d.TryGetValue("y", out var y) ? y : rt.anchorMax.y);
                }

                if (args.TryGetValue("pivot", out var pivotVal))
                {
                    var d = JsonConvert.DeserializeObject<Dictionary<string, float>>(pivotVal.ToString());
                    if (d != null)
                        rt.pivot = new Vector2(
                            d.TryGetValue("x", out var x) ? x : rt.pivot.x,
                            d.TryGetValue("y", out var y) ? y : rt.pivot.y);
                }

                return JsonConvert.SerializeObject(new
                {
                    success    = true,
                    instanceId = go.GetInstanceID(),
                    name       = go.name
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
