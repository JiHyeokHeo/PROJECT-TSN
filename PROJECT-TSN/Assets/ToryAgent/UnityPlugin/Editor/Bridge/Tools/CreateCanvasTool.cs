using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreateCanvasTool : IUnityEditorTool
    {
        public string Name => "create_canvas";
        public string Description => "Creates a Canvas GameObject with CanvasScaler and GraphicRaycaster. Returns instanceId.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"renderMode\":{\"type\":\"string\",\"enum\":[\"ScreenSpaceOverlay\",\"ScreenSpaceCamera\",\"WorldSpace\"],\"description\":\"Default: ScreenSpaceOverlay\"}},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}") ?? new Dictionary<string, object>();

                var goName = args.TryGetValue("name", out var n) ? n.ToString() : "Canvas";

                var go = new GameObject(goName);
                Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                if (args.TryGetValue("renderMode", out var rmVal))
                {
                    switch (rmVal.ToString())
                    {
                        case "ScreenSpaceCamera": canvas.renderMode = RenderMode.ScreenSpaceCamera; break;
                        case "WorldSpace":        canvas.renderMode = RenderMode.WorldSpace;        break;
                        default:                  canvas.renderMode = RenderMode.ScreenSpaceOverlay; break;
                    }
                }

                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode            = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution     = new Vector2(1920, 1080);
                scaler.screenMatchMode         = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight      = 0.5f;

                go.AddComponent<GraphicRaycaster>();

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
