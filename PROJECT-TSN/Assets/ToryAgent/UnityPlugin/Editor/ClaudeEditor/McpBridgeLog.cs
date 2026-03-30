using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    /// <summary>
    /// Static in-memory log for all MCP tool executions routed through ToryBridgeServer.
    /// </summary>
    public static class McpBridgeLog
    {
        public const int MaxEntries = 200;

        private static readonly List<McpLogEntry> _entries = new();

        public static IReadOnlyList<McpLogEntry> Entries => _entries;

        /// <summary>Fired on the calling thread whenever a new entry is added or the log is cleared.</summary>
        public static event Action OnLogUpdated;

        public static void AddEntry(McpLogEntry entry)
        {
            _entries.Add(entry);
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);
            OnLogUpdated?.Invoke();
        }

        public static void Clear()
        {
            _entries.Clear();
            OnLogUpdated?.Invoke();
        }
    }
}
