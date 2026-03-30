using System;

namespace ToryAgent.UnityPlugin.Editor
{
    [Serializable]
    public sealed class ToryBridgeRequest
    {
        public string toolName;
        public string argumentsJson;
    }
}