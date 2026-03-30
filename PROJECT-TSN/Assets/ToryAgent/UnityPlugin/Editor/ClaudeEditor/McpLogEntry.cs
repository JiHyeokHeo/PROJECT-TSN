using System;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class McpLogEntry
    {
        public DateTime Timestamp;
        public string ToolName = "";
        public string ArgumentsJson = "{}";
        public string ResultJson = "";
        public string ErrorMessage = "";
        public bool Success;
        public long DurationMs;
    }
}
