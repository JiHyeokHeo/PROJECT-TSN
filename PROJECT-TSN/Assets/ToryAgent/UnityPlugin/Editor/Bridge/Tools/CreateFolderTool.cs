using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreateFolderTool : IUnityEditorTool
    {
        public string Name => "create_folder";
        public string Description => "Creates a new folder in the Unity Asset database.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"parentPath\":{\"type\":\"string\",\"description\":\"Parent folder path e.g. Assets/Materials\"},\"folderName\":{\"type\":\"string\",\"description\":\"Name of the new folder\"}},\"required\":[\"parentPath\",\"folderName\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (!args.TryGetValue("parentPath", out var parentVal) || !args.TryGetValue("folderName", out var nameVal))
                    return JsonConvert.SerializeObject(new { error = "parentPath and folderName are required" });

                string parent = parentVal.ToString().TrimEnd('/');
                string folderName = nameVal.ToString();
                string fullPath = parent + "/" + folderName;

                if (AssetDatabase.IsValidFolder(fullPath))
                    return JsonConvert.SerializeObject(new { success = true, path = fullPath, alreadyExists = true });

                string guid = AssetDatabase.CreateFolder(parent, folderName);
                AssetDatabase.Refresh();

                return JsonConvert.SerializeObject(new
                {
                    success = !string.IsNullOrEmpty(guid),
                    path = fullPath,
                    guid = guid
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
