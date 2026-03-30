using System;

namespace ToryAgent.UnityPlugin.Editor
{
    [Serializable]
    public sealed class ToryBridgeResponse
    {
        public bool success;
        public string resultJson;
        public string errorMessage;
    }
}