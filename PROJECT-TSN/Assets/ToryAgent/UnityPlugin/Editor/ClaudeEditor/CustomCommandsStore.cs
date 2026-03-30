using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;

namespace ToryAgent.UnityPlugin.Editor
{
    [Serializable]
    public sealed class CustomCommand
    {
        public string Id = Guid.NewGuid().ToString("N").Substring(0, 8);
        public string Name = "New Command";
        public string Description = "";
        public string PromptTemplate = "";
    }

    /// <summary>
    /// Persists custom commands in EditorPrefs as JSON.
    /// </summary>
    public static class CustomCommandsStore
    {
        private const string PrefsKey = "ToryAgent.CustomCommands";
        private static List<CustomCommand> _cache;

        public static List<CustomCommand> Load()
        {
            if (_cache != null)
                return _cache;

            var json = EditorPrefs.GetString(PrefsKey, "[]");
            try
            {
                _cache = JsonConvert.DeserializeObject<List<CustomCommand>>(json) ?? new List<CustomCommand>();
            }
            catch
            {
                _cache = new List<CustomCommand>();
            }
            return _cache;
        }

        public static void Save(List<CustomCommand> commands)
        {
            _cache = commands;
            EditorPrefs.SetString(PrefsKey, JsonConvert.SerializeObject(commands, Formatting.None));
        }

        public static void InvalidateCache()
        {
            _cache = null;
        }
    }
}
