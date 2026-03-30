using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    /// <summary>
    /// Unity 메뉴 항목을 코드에서 실행합니다.
    /// EditorApplication.ExecuteMenuItem을 호출하므로 반드시 메인 스레드에서 실행돼야 합니다.
    /// </summary>
    public sealed class ExecuteEditorMenuTool : IUnityEditorTool
    {
        public string Name => "execute_editor_menu";

        public string Description =>
            "Executes a Unity Editor menu item by its path (e.g. 'Tools/TST/Create Vessel Prefabs'). " +
            "The menu item must exist and be enabled. Returns success or an error message.";

        public string InputSchemaJson =>
            "{\"type\":\"object\"," +
            "\"properties\":{" +
            "\"menuPath\":{\"type\":\"string\",\"description\":\"Full menu path, e.g. Tools/TST/Create Vessel Prefabs\"}" +
            "}," +
            "\"required\":[\"menuPath\"]," +
            "\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("menuPath", out var pathVal) || string.IsNullOrWhiteSpace(pathVal?.ToString()))
                    return JsonConvert.SerializeObject(new { error = "menuPath is required" });

                string menuPath = pathVal.ToString();

                bool executed = EditorApplication.ExecuteMenuItem(menuPath);

                if (!executed)
                    return JsonConvert.SerializeObject(new
                    {
                        error = $"Menu item not found or not enabled: '{menuPath}'"
                    });

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    menuPath
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
