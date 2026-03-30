using UnityEngine;

namespace ToryAgent
{
    [CreateAssetMenu(
        fileName = "ClaudeEditorSettings",
        menuName = "ToryAgent/Claude Editor Settings",
        order = 100)]
    public class ClaudeEditorSettings : ScriptableObject
    {
        [Header("Claude CLI")]
        [SerializeField] private string claudeExecutablePath = "";
        [SerializeField] private bool useBypassPermissions = true;
        [SerializeField] private int timeoutSeconds = 120;
        [Tooltip("Maximum number of agentic turns. 1 = single response (no tool loops). Increase for multi-step tasks.")]
        [SerializeField] private int maxTurns = 1;

        [Header("Prompt")]
        [TextArea(3, 10)]
        [SerializeField] private string systemPrompt =
            "You are operating through Claude Code CLI inside a Unity project. " +
            "Use available MCP servers if needed and keep responses concise.";

        public string ClaudeExecutablePath => claudeExecutablePath;
        public bool UseBypassPermissions => useBypassPermissions;
        public int TimeoutSeconds => timeoutSeconds;
        public int MaxTurns => Mathf.Max(1, maxTurns);
        public string SystemPrompt => systemPrompt;
    }
}