using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using System;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SaveSceneTool : IUnityEditorTool
    {
        public string Name => "save_scene";
        public string Description => "Saves the currently active Unity scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                bool saved = EditorSceneManager.SaveScene(scene);
                return JsonConvert.SerializeObject(new
                {
                    success = saved,
                    sceneName = scene.name,
                    scenePath = scene.path
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
