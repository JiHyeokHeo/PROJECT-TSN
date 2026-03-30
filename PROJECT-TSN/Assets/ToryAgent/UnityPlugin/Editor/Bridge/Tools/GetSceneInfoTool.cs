using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class GetSceneInfoTool : IUnityEditorTool
    {
        public string Name => "get_scene_info";
        public string Description => "Returns information about the currently open Unity scene.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                return JsonConvert.SerializeObject(new
                {
                    name = scene.name,
                    path = scene.path,
                    isDirty = scene.isDirty,
                    isLoaded = scene.isLoaded,
                    rootCount = scene.rootCount,
                    buildIndex = scene.buildIndex
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
