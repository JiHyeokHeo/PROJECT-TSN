using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreateUIElementTool : IUnityEditorTool
    {
        public string Name => "create_ui_element";
        public string Description => "Creates a UI element (Panel, Button, Text, Image, InputField, Slider, Toggle, ScrollView) under a Canvas or UI parent.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"elementType\":{\"type\":\"string\",\"enum\":[\"Panel\",\"Button\",\"Text\",\"Image\",\"InputField\",\"Slider\",\"Toggle\",\"ScrollView\"]},\"name\":{\"type\":\"string\"},\"parentInstanceId\":{\"type\":\"number\",\"description\":\"InstanceId of parent Canvas or UI object\"},\"anchoredPosition\":{\"type\":\"object\",\"description\":\"{x,y}\"},\"sizeDelta\":{\"type\":\"object\",\"description\":\"{x,y}\"}},\"required\":[\"elementType\",\"parentInstanceId\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("elementType", out var etVal))
                    return JsonConvert.SerializeObject(new { error = "elementType is required" });
                if (!args.TryGetValue("parentInstanceId", out var pidVal))
                    return JsonConvert.SerializeObject(new { error = "parentInstanceId is required" });

                var parentGo = EditorUtility.EntityIdToObject(Convert.ToInt32(pidVal)) as GameObject;
                if (parentGo == null)
                    return JsonConvert.SerializeObject(new { error = "Parent GameObject not found" });

                var resources = new DefaultControls.Resources();
                GameObject created;

                switch (etVal.ToString())
                {
                    case "Panel":      created = DefaultControls.CreatePanel(resources);      break;
                    case "Button":     created = DefaultControls.CreateButton(resources);     break;
                    case "Text":       created = DefaultControls.CreateText(resources);       break;
                    case "Image":      created = DefaultControls.CreateImage(resources);      break;
                    case "InputField": created = DefaultControls.CreateInputField(resources); break;
                    case "Slider":     created = DefaultControls.CreateSlider(resources);     break;
                    case "Toggle":     created = DefaultControls.CreateToggle(resources);     break;
                    case "ScrollView": created = DefaultControls.CreateScrollView(resources); break;
                    default:
                        return JsonConvert.SerializeObject(new { error = $"Unknown elementType: {etVal}" });
                }

                Undo.RegisterCreatedObjectUndo(created, $"Create UI {etVal}");
                created.transform.SetParent(parentGo.transform, false);

                if (args.TryGetValue("name", out var nameVal))
                    created.name = nameVal.ToString();

                var rt = created.GetComponent<RectTransform>();
                if (rt != null)
                {
                    if (args.TryGetValue("anchoredPosition", out var apVal))
                    {
                        var ap = JsonConvert.DeserializeObject<Dictionary<string, float>>(apVal.ToString());
                        if (ap != null)
                            rt.anchoredPosition = new Vector2(
                                ap.TryGetValue("x", out var ax) ? ax : rt.anchoredPosition.x,
                                ap.TryGetValue("y", out var ay) ? ay : rt.anchoredPosition.y);
                    }
                    if (args.TryGetValue("sizeDelta", out var sdVal))
                    {
                        var sd = JsonConvert.DeserializeObject<Dictionary<string, float>>(sdVal.ToString());
                        if (sd != null)
                            rt.sizeDelta = new Vector2(
                                sd.TryGetValue("x", out var sx) ? sx : rt.sizeDelta.x,
                                sd.TryGetValue("y", out var sy) ? sy : rt.sizeDelta.y);
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    success    = true,
                    instanceId = created.GetInstanceID(),
                    name       = created.name
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
