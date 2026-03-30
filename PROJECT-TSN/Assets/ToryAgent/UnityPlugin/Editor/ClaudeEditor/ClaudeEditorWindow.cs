using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class ClaudeEditorWindow : EditorWindow
    {
        // ─── Constants ───────────────────────────────────────────────────────────
        private const string DefaultSettingsPath =
            "Assets/PROJECT-A/Resources/ToryAgent/ClaudeEditorSettings.asset";

        private static readonly string[] TabNames = { "Prompt", "MCP Tools", "Log", "Commands" };

        // ─── Shared state ────────────────────────────────────────────────────────
        private readonly ClaudeEditorService _service = new ClaudeEditorService();
        private ClaudeEditorSettings _settings;
        private int _selectedTab;

        // ─── Prompt tab ──────────────────────────────────────────────────────────
        private string _prompt = "Select a GameObject and describe the task.";
        private string _response = "";
        private string _stderr = "";
        private string _executionLog = "";
        private bool _lastRunFailed;
        private Vector2 _responseScroll;
        private Vector2 _stderrScroll;
        private bool _isSending;

        // ─── MCP Tools tab ───────────────────────────────────────────────────────
        [Serializable]
        private sealed class ToolInfo
        {
            public string name = "";
            public string description = "";
            public string inputSchemaJson = "{}";
        }

        [Serializable]
        private sealed class ToolListResponse { public List<ToolInfo> tools = new List<ToolInfo>(); }

        private List<ToolInfo> _mcpTools = new List<ToolInfo>();
        private string[] _toolArgInputs = Array.Empty<string>();
        private string _mcpToolResult = "";
        private string _mcpToolError = "";
        private bool _isRefreshingTools;
        private bool _isExecutingTool;
        private int _selectedToolIndex = -1;
        private Vector2 _toolsScroll;

        // ─── Log tab ─────────────────────────────────────────────────────────────
        private Vector2 _logScroll;
        private int _selectedLogIndex = -1;
        private bool _autoScrollLog = true;

        // ─── Commands tab ────────────────────────────────────────────────────────
        private List<CustomCommand> _customCommands;
        private bool _isAddingCommand;
        private int _editingCommandIndex = -1;
        private string _editName = "";
        private string _editDesc = "";
        private string _editPrompt = "";
        private Vector2 _commandsScroll;

        // ─── Colors (cached) ─────────────────────────────────────────────────────
        private static readonly Color ColorSuccess = new Color(0.4f, 1.0f, 0.5f);
        private static readonly Color ColorFailBg  = new Color(1.0f, 0.85f, 0.85f);
        private static readonly Color ColorFailText = new Color(1.0f, 0.4f, 0.4f);
        private static readonly Color ColorSelected = new Color(0.65f, 0.82f, 1.0f);

        // ═════════════════════════════════════════════════════════════════════════
        [MenuItem("ToryAgent/Claude Editor")]
        public static void Open()
        {
            var w = GetWindow<ClaudeEditorWindow>("Claude Editor");
            w.minSize = new Vector2(620f, 560f);
            w.Show();
        }

        private void OnEnable()
        {
            _settings = FindSettings();
            _customCommands = CustomCommandsStore.Load();
            McpBridgeLog.OnLogUpdated += OnLogUpdated;
        }

        private void OnDisable()
        {
            McpBridgeLog.OnLogUpdated -= OnLogUpdated;
        }

        private void OnLogUpdated() => EditorApplication.delayCall += Repaint;

        // ─── Top-level GUI ────────────────────────────────────────────────────────
        private void OnGUI()
        {
            DrawTabBar();
            EditorGUILayout.Space(4);

            switch (_selectedTab)
            {
                case 0: DrawPromptTab();   break;
                case 1: DrawMcpToolsTab(); break;
                case 2: DrawLogTab();      break;
                case 3: DrawCommandsTab(); break;
            }
        }

        private void DrawTabBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                for (int i = 0; i < TabNames.Length; i++)
                {
                    string label = TabNames[i];
                    if (i == 2 && McpBridgeLog.Entries.Count > 0)
                        label = $"Log ({McpBridgeLog.Entries.Count})";

                    bool active = _selectedTab == i;
                    if (GUILayout.Toggle(active, label, EditorStyles.toolbarButton) && !active)
                        _selectedTab = i;
                }
                GUILayout.FlexibleSpace();
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // TAB 0 — PROMPT
        // ═════════════════════════════════════════════════════════════════════════

        private void DrawPromptTab()
        {
            // Settings
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Claude CLI Settings", EditorStyles.boldLabel);
                _settings = (ClaudeEditorSettings)EditorGUILayout.ObjectField(
                    "Settings", _settings, typeof(ClaudeEditorSettings), false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Find"))   { _settings = FindSettings(); Repaint(); }
                    if (GUILayout.Button("Create")) { _settings = CreateSettingsAsset(); Repaint(); }
                    GUI.enabled = _settings != null;
                    if (GUILayout.Button("Ping")) EditorGUIUtility.PingObject(_settings);
                    GUI.enabled = true;
                }

                EditorGUILayout.HelpBox(
                    "Runs Claude CLI as a subprocess — uses your local login token and all configured MCP servers.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(6);

            // Prompt input
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.MinHeight(90f));

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = !_isSending && _settings != null;
                    if (GUILayout.Button("Run Claude CLI", GUILayout.Height(26f)))
                        _ = RunPromptAsync();

                    GUI.enabled = !_isSending;
                    if (GUILayout.Button("Clear", GUILayout.Height(26f), GUILayout.Width(60f)))
                    {
                        _response = ""; _stderr = ""; _executionLog = "";
                    }
                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();
                    if (_isSending)
                        EditorGUILayout.LabelField("Running...", EditorStyles.miniLabel, GUILayout.Width(70f));
                }
            }

            EditorGUILayout.Space(6);

            // Output
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Claude Output", EditorStyles.boldLabel);
                _responseScroll = EditorGUILayout.BeginScrollView(_responseScroll, GUILayout.MinHeight(150f));
                EditorGUILayout.SelectableLabel(
                    string.IsNullOrWhiteSpace(_response) ? "(No output yet)" : _response,
                    EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(4);

            // Stderr + status
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("CLI Logs / Status", EditorStyles.boldLabel);

                if (!string.IsNullOrWhiteSpace(_executionLog))
                    EditorGUILayout.LabelField(_executionLog, EditorStyles.miniLabel);

                if (!string.IsNullOrWhiteSpace(_stderr))
                {
                    _stderrScroll = EditorGUILayout.BeginScrollView(_stderrScroll, GUILayout.MinHeight(60f));
                    // Only color red on actual failure — Claude CLI writes info/progress to stderr even on success.
                    if (_lastRunFailed)
                    {
                        var prevCol = GUI.color;
                        GUI.color = ColorFailText;
                        EditorGUILayout.SelectableLabel(_stderr, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                        GUI.color = prevCol;
                    }
                    else
                    {
                        EditorGUILayout.SelectableLabel(_stderr, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private async Task RunPromptAsync()
        {
            if (_settings == null) { _executionLog = "Settings asset missing."; Repaint(); return; }
            if (string.IsNullOrWhiteSpace(_prompt)) { _executionLog = "Prompt is empty."; Repaint(); return; }

            _isSending = true;
            _executionLog = "Running Claude CLI...";
            _response = ""; _stderr = ""; _lastRunFailed = false;
            Repaint();

            try
            {
                var cwd = Directory.GetParent(Application.dataPath)?.FullName ?? Environment.CurrentDirectory;
                var fullPrompt = BuildPromptWithSelection(_prompt);
                var result = await _service.SendAsync(_settings, fullPrompt, cwd);

                _response = string.IsNullOrWhiteSpace(result.StandardOutput)
                    ? "(empty stdout)" : result.StandardOutput;
                _stderr = result.StandardError ?? "";
                _lastRunFailed = !result.Success;
                _executionLog = result.Success
                    ? $"Done (exit={result.ExitCode})"
                    : $"Failed (exit={result.ExitCode})";
            }
            catch (Exception ex)
            {
                _stderr = ex.ToString();
                _lastRunFailed = true;
                _executionLog = "CLI run failed.";
            }
            finally
            {
                _isSending = false;
                Repaint();
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // TAB 1 — MCP TOOLS
        // ═════════════════════════════════════════════════════════════════════════

        private void DrawMcpToolsTab()
        {
            // Header row
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"Bridge Tools  (port {ToryBridgeServer.DefaultPort})", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUI.enabled = !_isRefreshingTools && !_isExecutingTool;
                if (GUILayout.Button(_isRefreshingTools ? "Refreshing..." : "Refresh", GUILayout.Width(90)))
                    _ = RefreshToolsAsync();
                GUI.enabled = true;
            }

            if (_mcpTools.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No tools loaded. Make sure the ToryBridgeServer is running (auto-starts with editor), then click Refresh.",
                    MessageType.Info);
                return;
            }

            // Tool list
            _toolsScroll = EditorGUILayout.BeginScrollView(_toolsScroll,
                GUILayout.MinHeight(200f), GUILayout.ExpandHeight(true));

            for (int i = 0; i < _mcpTools.Count; i++)
            {
                var tool = _mcpTools[i];
                bool isSelected = _selectedToolIndex == i;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    // Tool header row
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        string arrow = isSelected ? "▼" : "▶";
                        if (GUILayout.Button($"{arrow}  {tool.name}", EditorStyles.boldLabel,
                            GUILayout.ExpandWidth(true)))
                            _selectedToolIndex = isSelected ? -1 : i;

                        GUILayout.FlexibleSpace();

                        GUI.enabled = !_isExecutingTool && !_isRefreshingTools;
                        if (GUILayout.Button("Execute", GUILayout.Width(70)))
                        {
                            _selectedToolIndex = i;
                            _ = ExecuteToolAsync(i);
                        }
                        GUI.enabled = true;
                    }

                    if (!string.IsNullOrWhiteSpace(tool.description))
                        EditorGUILayout.LabelField(tool.description, EditorStyles.wordWrappedMiniLabel);

                    // Expandable args
                    if (isSelected)
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("Arguments (JSON):", EditorStyles.miniLabel);
                        _toolArgInputs[i] = EditorGUILayout.TextArea(
                            _toolArgInputs[i], GUILayout.MinHeight(44f));

                        if (!string.IsNullOrWhiteSpace(tool.inputSchemaJson) && tool.inputSchemaJson != "{}")
                        {
                            EditorGUILayout.LabelField("Schema:", EditorStyles.miniLabel);
                            EditorGUILayout.SelectableLabel(tool.inputSchemaJson,
                                EditorStyles.wordWrappedMiniLabel, GUILayout.MinHeight(30f));
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            // Last result panel
            if (!string.IsNullOrWhiteSpace(_mcpToolResult) || !string.IsNullOrWhiteSpace(_mcpToolError))
            {
                EditorGUILayout.Space(4);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Last Result", EditorStyles.boldLabel);
                    if (!string.IsNullOrWhiteSpace(_mcpToolError))
                    {
                        var prev = GUI.color; GUI.color = ColorFailText;
                        EditorGUILayout.SelectableLabel(_mcpToolError,
                            EditorStyles.wordWrappedLabel, GUILayout.MinHeight(40f));
                        GUI.color = prev;
                    }
                    else
                    {
                        EditorGUILayout.SelectableLabel(_mcpToolResult,
                            EditorStyles.wordWrappedLabel, GUILayout.MinHeight(40f));
                    }
                }
            }
        }

        private async Task RefreshToolsAsync()
        {
            _isRefreshingTools = true;
            _mcpTools = new List<ToolInfo>();
            _toolArgInputs = Array.Empty<string>();
            _mcpToolResult = "";
            _mcpToolError = "";
            _selectedToolIndex = -1;
            Repaint();

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var json = await client.GetStringAsync(
                    $"http://127.0.0.1:{ToryBridgeServer.DefaultPort}/tory/tools/list");
                var parsed = JsonConvert.DeserializeObject<ToolListResponse>(json);
                _mcpTools = parsed?.tools ?? new List<ToolInfo>();
                _toolArgInputs = new string[_mcpTools.Count];
                for (int i = 0; i < _toolArgInputs.Length; i++) _toolArgInputs[i] = "{}";
                if (_mcpTools.Count > 0) _selectedToolIndex = 0;
            }
            catch (Exception ex)
            {
                _mcpToolError = $"Refresh failed: {ex.Message}";
            }
            finally
            {
                _isRefreshingTools = false;
                Repaint();
            }
        }

        private async Task ExecuteToolAsync(int index)
        {
            if (index < 0 || index >= _mcpTools.Count) return;
            var tool = _mcpTools[index];

            _isExecutingTool = true;
            _mcpToolResult = "";
            _mcpToolError = $"Executing '{tool.name}'...";
            Repaint();

            try
            {
                var body = JsonConvert.SerializeObject(new
                {
                    toolName = tool.name,
                    argumentsJson = _toolArgInputs[index]
                });
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var httpResp = await client.PostAsync(
                    $"http://127.0.0.1:{ToryBridgeServer.DefaultPort}/tory/tools/execute", content);
                var respJson = await httpResp.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<ToryBridgeResponse>(respJson);

                if (parsed != null && parsed.success)
                {
                    _mcpToolResult = parsed.resultJson;
                    _mcpToolError = "";
                }
                else
                {
                    _mcpToolError = parsed?.errorMessage ?? "Unknown error";
                }
            }
            catch (Exception ex)
            {
                _mcpToolError = ex.Message;
            }
            finally
            {
                _isExecutingTool = false;
                Repaint();
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // TAB 2 — LOG VIEWER
        // ═════════════════════════════════════════════════════════════════════════

        private void DrawLogTab()
        {
            var entries = McpBridgeLog.Entries;

            // Header
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"MCP Call Log  ({entries.Count} entries)", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                _autoScrollLog = GUILayout.Toggle(_autoScrollLog, "Auto-scroll", GUILayout.Width(90));
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    McpBridgeLog.Clear();
                    _selectedLogIndex = -1;
                }
            }

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No MCP calls recorded yet. Tool executions through the bridge appear here automatically.",
                    MessageType.Info);
                return;
            }

            // Adaptive height: give ~45% to list if detail panel is open
            float listH = _selectedLogIndex >= 0
                ? Mathf.Max(120f, position.height * 0.42f)
                : position.height - 100f;

            _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.Height(listH));

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                bool isSelected = _selectedLogIndex == i;

                string icon  = e.Success ? "✓" : "✗";
                string label = $"{icon}  [{e.Timestamp:HH:mm:ss.fff}]  {e.ToolName}  ({e.DurationMs} ms)";

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = isSelected ? ColorSelected : (e.Success ? Color.white : ColorFailBg);

                if (GUILayout.Button(label, EditorStyles.helpBox))
                    _selectedLogIndex = isSelected ? -1 : i;

                GUI.backgroundColor = prevColor;
            }

            if (_autoScrollLog) _logScroll.y = float.MaxValue;
            EditorGUILayout.EndScrollView();

            // Detail panel
            if (_selectedLogIndex >= 0 && _selectedLogIndex < entries.Count)
            {
                var e = entries[_selectedLogIndex];
                EditorGUILayout.Space(4);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Detail  ·  {e.ToolName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}   {e.DurationMs} ms   " +
                        (e.Success ? "Success" : "Failed"),
                        EditorStyles.miniLabel);
                    EditorGUILayout.Space(2);

                    EditorGUILayout.LabelField("Arguments:", EditorStyles.miniLabel);
                    EditorGUILayout.SelectableLabel(
                        string.IsNullOrWhiteSpace(e.ArgumentsJson) ? "{}" : e.ArgumentsJson,
                        EditorStyles.wordWrappedLabel, GUILayout.MinHeight(28f));

                    if (e.Success && !string.IsNullOrWhiteSpace(e.ResultJson))
                    {
                        EditorGUILayout.LabelField("Result:", EditorStyles.miniLabel);
                        EditorGUILayout.SelectableLabel(e.ResultJson,
                            EditorStyles.wordWrappedLabel, GUILayout.MinHeight(40f));
                    }
                    else if (!string.IsNullOrWhiteSpace(e.ErrorMessage))
                    {
                        EditorGUILayout.LabelField("Error:", EditorStyles.miniLabel);
                        var prev = GUI.color; GUI.color = ColorFailText;
                        EditorGUILayout.SelectableLabel(e.ErrorMessage,
                            EditorStyles.wordWrappedLabel, GUILayout.MinHeight(28f));
                        GUI.color = prev;
                    }
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // TAB 3 — CUSTOM COMMANDS
        // ═════════════════════════════════════════════════════════════════════════

        private void DrawCommandsTab()
        {
            // Header
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"Custom Commands  ({_customCommands.Count})", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ New", GUILayout.Width(60)))
                {
                    _isAddingCommand = true;
                    _editingCommandIndex = -1;
                    _editName = "New Command";
                    _editDesc = "";
                    _editPrompt = "";
                }
            }

            // Edit/Add form (appears at the top when active)
            if (_isAddingCommand || _editingCommandIndex >= 0)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(
                        _isAddingCommand ? "— New Command —" : "— Edit Command —",
                        EditorStyles.boldLabel);
                    _editName   = EditorGUILayout.TextField("Name", _editName);
                    _editDesc   = EditorGUILayout.TextField("Description", _editDesc);
                    EditorGUILayout.LabelField("Prompt Template:", EditorStyles.miniLabel);
                    _editPrompt = EditorGUILayout.TextArea(_editPrompt, GUILayout.MinHeight(70f));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Save", GUILayout.Height(24)))
                        {
                            if (_isAddingCommand)
                            {
                                _customCommands.Add(new CustomCommand
                                {
                                    Name = _editName,
                                    Description = _editDesc,
                                    PromptTemplate = _editPrompt
                                });
                            }
                            else
                            {
                                var cmd = _customCommands[_editingCommandIndex];
                                cmd.Name = _editName;
                                cmd.Description = _editDesc;
                                cmd.PromptTemplate = _editPrompt;
                            }
                            CustomCommandsStore.Save(_customCommands);
                            _isAddingCommand = false;
                            _editingCommandIndex = -1;
                        }

                        if (GUILayout.Button("Cancel", GUILayout.Height(24)))
                        {
                            _isAddingCommand = false;
                            _editingCommandIndex = -1;
                        }
                    }
                }
                EditorGUILayout.Space(4);
            }

            // Command list
            if (_customCommands.Count == 0 && !_isAddingCommand)
            {
                EditorGUILayout.HelpBox(
                    "No commands yet. Click '+ New' to create a reusable prompt template.",
                    MessageType.Info);
                return;
            }

            _commandsScroll = EditorGUILayout.BeginScrollView(_commandsScroll);
            for (int i = 0; i < _customCommands.Count; i++)
            {
                var cmd = _customCommands[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(cmd.Name, EditorStyles.boldLabel,
                            GUILayout.ExpandWidth(true));

                        // Run — sends to Prompt tab immediately
                        GUI.enabled = !_isSending && _settings != null;
                        if (GUILayout.Button("Run", GUILayout.Width(40)))
                        {
                            _prompt = cmd.PromptTemplate;
                            _selectedTab = 0;
                            _ = RunPromptAsync();
                        }
                        GUI.enabled = true;

                        // Fill prompt (without running)
                        if (GUILayout.Button("Fill", GUILayout.Width(36)))
                        {
                            _prompt = cmd.PromptTemplate;
                            _selectedTab = 0;
                        }

                        // Edit
                        if (GUILayout.Button("Edit", GUILayout.Width(38)))
                        {
                            _editingCommandIndex = i;
                            _isAddingCommand = false;
                            _editName   = cmd.Name;
                            _editDesc   = cmd.Description;
                            _editPrompt = cmd.PromptTemplate;
                        }

                        // Delete
                        var prevBg = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                        if (GUILayout.Button("✕", GUILayout.Width(22)))
                        {
                            _customCommands.RemoveAt(i);
                            CustomCommandsStore.Save(_customCommands);
                            if (_editingCommandIndex == i) _editingCommandIndex = -1;
                            GUI.backgroundColor = prevBg;
                            break;
                        }
                        GUI.backgroundColor = prevBg;
                    }

                    if (!string.IsNullOrWhiteSpace(cmd.Description))
                        EditorGUILayout.LabelField(cmd.Description, EditorStyles.wordWrappedMiniLabel);

                    if (!string.IsNullOrWhiteSpace(cmd.PromptTemplate))
                    {
                        var preview = cmd.PromptTemplate.Length > 100
                            ? cmd.PromptTemplate.Substring(0, 100) + "…"
                            : cmd.PromptTemplate;
                        EditorGUILayout.LabelField(preview, EditorStyles.wordWrappedMiniLabel);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        // ═════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════════

        private static ClaudeEditorSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:ClaudeEditorSettings");
            if (guids == null || guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<ClaudeEditorSettings>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static ClaudeEditorSettings CreateSettingsAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ClaudeEditorSettings>(DefaultSettingsPath);
            if (existing != null) { Selection.activeObject = existing; return existing; }

            var dir = Path.GetDirectoryName(DefaultSettingsPath);
            if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
                CreateFolderRecursively(dir);

            var asset = CreateInstance<ClaudeEditorSettings>();
            AssetDatabase.CreateAsset(asset, DefaultSettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        private static void CreateFolderRecursively(string fullPath)
        {
            var segments = fullPath.Replace('\\', '/').Split('/');
            var current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
        }

        private static string BuildPromptWithSelection(string userPrompt)
        {
            var sb = new StringBuilder();
            sb.AppendLine(userPrompt.Trim());
            sb.AppendLine();
            sb.AppendLine("[Unity Editor Context]");

            var selected = Selection.activeTransform;
            if (selected == null)
            {
                sb.AppendLine("No active GameObject selection.");
            }
            else
            {
                var pos = selected.position;
                sb.AppendLine($"SelectedName: {selected.name}");
                sb.AppendLine($"SelectedPosition: {pos.x:F3}, {pos.y:F3}, {pos.z:F3}");
            }

            return sb.ToString();
        }
    }
}
