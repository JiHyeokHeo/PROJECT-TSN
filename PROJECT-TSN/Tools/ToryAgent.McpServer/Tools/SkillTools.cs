using System.Text.Json.Nodes;
using System.Text;
using ToryAgent.McpServer.Protocol;

namespace ToryAgent.McpServer.Tools;

// ── Helpers ──────────────────────────────────────────────────────────────────

file static class SkillHelper
{
    /// <summary>
    /// Walks up from cwd until it finds a directory containing a ".claude" folder.
    /// Returns the .claude/skills path, or null if not found.
    /// </summary>
    internal static string? FindSkillsDir(string scope)
    {
        if (scope == "global")
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".claude", "skills");
        }

        // Project scope: walk up from cwd to find .claude folder
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".claude");
            if (Directory.Exists(candidate))
                return Path.Combine(candidate, "skills");
            dir = dir.Parent;
        }
        return null;
    }

    internal static ToolCallResult Ok(string text) => new()
    {
        Content = new object[] { new { type = "text", text } },
        IsError = false
    };

    internal static ToolCallResult Err(string message) => new()
    {
        Content = new object[] { new { type = "text", text = $"Error: {message}" } },
        IsError = true
    };
}

// ── list_skills ───────────────────────────────────────────────────────────────

public sealed class ListSkillsTool : IMcpTool
{
    public string Name => "list_skills";
    public string Description => "Lists all available Claude Code skills. Returns skill names, scopes (project/global), and their description lines.";
    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["scope"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("project", "global", "all"),
                ["description"] = "Which skills to list (default: all)"
            }
        },
        ["additionalProperties"] = false
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        var scope = arguments["scope"]?.GetValue<string>() ?? "all";

        var scopes = scope == "all"
            ? new[] { "project", "global" }
            : new[] { scope };

        var skills = new List<object>();

        foreach (var s in scopes)
        {
            var dir = SkillHelper.FindSkillsDir(s);
            if (dir == null || !Directory.Exists(dir))
                continue;

            foreach (var file in Directory.GetFiles(dir, "*.md").OrderBy(f => f))
            {
                var skillName = Path.GetFileNameWithoutExtension(file);
                var firstLine = File.ReadLines(file).FirstOrDefault(l => !l.StartsWith("---") && !string.IsNullOrWhiteSpace(l)) ?? "";
                skills.Add(new { scope = s, name = skillName, command = $"/{skillName}", description = firstLine.TrimStart('#').Trim() });
            }
        }

        var json = System.Text.Json.JsonSerializer.Serialize(skills, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        return Task.FromResult(SkillHelper.Ok(json));
    }
}

// ── read_skill ────────────────────────────────────────────────────────────────

public sealed class ReadSkillTool : IMcpTool
{
    public string Name => "read_skill";
    public string Description => "Reads the full content of a Claude Code skill file.";
    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string", ["description"] = "Skill name (without .md or /)" },
            ["scope"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("project", "global"), ["description"] = "Skill scope (default: project)" }
        },
        ["required"] = new JsonArray("name"),
        ["additionalProperties"] = false
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        var name = arguments["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(SkillHelper.Err("name is required"));

        var scope = arguments["scope"]?.GetValue<string>() ?? "project";
        var dir = SkillHelper.FindSkillsDir(scope);
        if (dir == null)
            return Task.FromResult(SkillHelper.Err($"Could not find .claude/skills directory for scope '{scope}'"));

        var path = Path.Combine(dir, $"{name.TrimStart('/')}.md");
        if (!File.Exists(path))
            return Task.FromResult(SkillHelper.Err($"Skill '{name}' not found at: {path}"));

        return Task.FromResult(SkillHelper.Ok(File.ReadAllText(path)));
    }
}

// ── create_skill ──────────────────────────────────────────────────────────────

public sealed class CreateSkillTool : IMcpTool
{
    public string Name => "create_skill";
    public string Description => "Creates or overwrites a Claude Code skill file (.claude/skills/<name>.md). The content becomes the prompt that Claude receives when the skill is invoked.";
    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string", ["description"] = "Skill name — becomes the slash command /name (no spaces, use hyphens)" },
            ["content"] = new JsonObject { ["type"] = "string", ["description"] = "Full markdown content of the skill prompt" },
            ["scope"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("project", "global"), ["description"] = "project = .claude/skills/, global = ~/.claude/skills/ (default: project)" }
        },
        ["required"] = new JsonArray("name", "content"),
        ["additionalProperties"] = false
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        var name = arguments["name"]?.GetValue<string>();
        var content = arguments["content"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(SkillHelper.Err("name is required"));
        if (content == null)
            return Task.FromResult(SkillHelper.Err("content is required"));

        // Sanitize: remove leading slash, disallow path traversal
        name = name.TrimStart('/');
        if (name.Contains('/') || name.Contains('\\') || name.Contains(".."))
            return Task.FromResult(SkillHelper.Err("Skill name must not contain path separators"));

        var scope = arguments["scope"]?.GetValue<string>() ?? "project";
        var dir = SkillHelper.FindSkillsDir(scope);
        if (dir == null)
            return Task.FromResult(SkillHelper.Err($"Could not find .claude directory for scope '{scope}'"));

        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{name}.md");
        var existed = File.Exists(path);

        File.WriteAllText(path, content, Encoding.UTF8);

        return Task.FromResult(SkillHelper.Ok(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                action  = existed ? "overwritten" : "created",
                command = $"/{name}",
                path,
                scope
            })));
    }
}

// ── delete_skill ──────────────────────────────────────────────────────────────

public sealed class DeleteSkillTool : IMcpTool
{
    public string Name => "delete_skill";
    public string Description => "Deletes a Claude Code skill file.";
    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string", ["description"] = "Skill name to delete (without .md or /)" },
            ["scope"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("project", "global"), ["description"] = "Skill scope (default: project)" }
        },
        ["required"] = new JsonArray("name"),
        ["additionalProperties"] = false
    };

    public Task<ToolCallResult> ExecuteAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        var name = arguments["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(SkillHelper.Err("name is required"));

        name = name.TrimStart('/');
        if (name.Contains('/') || name.Contains('\\') || name.Contains(".."))
            return Task.FromResult(SkillHelper.Err("Skill name must not contain path separators"));

        var scope = arguments["scope"]?.GetValue<string>() ?? "project";
        var dir = SkillHelper.FindSkillsDir(scope);
        if (dir == null)
            return Task.FromResult(SkillHelper.Err($"Could not find .claude directory for scope '{scope}'"));

        var path = Path.Combine(dir, $"{name}.md");
        if (!File.Exists(path))
            return Task.FromResult(SkillHelper.Err($"Skill '{name}' not found"));

        File.Delete(path);

        return Task.FromResult(SkillHelper.Ok(
            System.Text.Json.JsonSerializer.Serialize(new { success = true, deleted = $"/{name}", path })));
    }
}
