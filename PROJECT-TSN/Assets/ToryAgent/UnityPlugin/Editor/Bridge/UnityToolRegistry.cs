using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class UnityToolRegistry
    {
        readonly Dictionary<string, IUnityEditorTool> toolMap = new(StringComparer.Ordinal);

        public UnityToolRegistry(IEnumerable<IUnityEditorTool> tools)
        {
            foreach (IUnityEditorTool tool in tools)
                toolMap[tool.Name] = tool;
        }

        public IReadOnlyCollection<IUnityEditorTool> GetAll()
        {
            return toolMap.Values;
        }

        public bool TryGet(string name, out IUnityEditorTool tool)
        {
            return toolMap.TryGetValue(name, out tool);
        }
    }
}