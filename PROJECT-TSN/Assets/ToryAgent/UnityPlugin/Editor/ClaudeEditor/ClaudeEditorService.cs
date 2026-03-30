using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ToryAgent.UnityPlugin.Editor
{
    internal sealed class ClaudeEditorService
    {
        internal sealed class ClaudeCliResult
        {
            public bool Success;
            public int ExitCode;
            public string StandardOutput = "";
            public string StandardError = "";
            public string Command = "";
        }

        public async Task<ClaudeCliResult> SendAsync(
            ClaudeEditorSettings settings,
            string userPrompt,
            string workingDirectory)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("Prompt is empty.", nameof(userPrompt));

            var claudePath = ResolveClaudePath(settings.ClaudeExecutablePath);
            var timeoutSec = Mathf.Max(5, settings.TimeoutSeconds);

            // No quoted values in args — avoids all cmd.exe quoting issues on Windows.
            var args = BuildArguments(settings);

            // System prompt is prepended to stdin instead of passed via
            // --append-system-prompt, which would require quoting inside cmd.exe /C.
            var stdinContent = BuildStdin(settings.SystemPrompt, userPrompt);

            var result = await Task.Run(() =>
                RunProcess(claudePath, args, workingDirectory, timeoutSec, stdinContent));

            result.Command = $"{claudePath} {args}  (content via stdin)";
            return result;
        }

        // ── Process runner ───────────────────────────────────────────────────────

        private static ClaudeCliResult RunProcess(
            string fileName,
            string arguments,
            string cwd,
            int timeoutSec,
            string stdinInput)
        {
            var isWindows   = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isCmdScript = isWindows &&
                              (fileName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ||
                               fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase));

            // For .cmd/.bat: cmd.exe /C "script.cmd" args
            // Having EXACTLY ONE quoted token (the script path) is key —
            // cmd.exe preserves quotes when there are exactly 2 quote chars total
            // and no special characters between them. Adding extra quoted flags
            // (like --append-system-prompt "...") breaks this rule and causes
            // cmd.exe to mis-parse the entire command line.
            string exeName, exeArgs;
            if (isCmdScript)
            {
                exeName = "cmd.exe";
                exeArgs = $"/C \"{fileName}\" {arguments}";
            }
            else
            {
                exeName = fileName;
                exeArgs = arguments;
            }

            var psi = new ProcessStartInfo
            {
                FileName               = exeName,
                Arguments              = exeArgs,
                WorkingDirectory       = string.IsNullOrWhiteSpace(cwd)
                                             ? Environment.CurrentDirectory : cwd,
                UseShellExecute        = false,
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding  = Encoding.UTF8,
            };

            // Remove API key so Claude Code falls back to OAuth/subscription auth.
            // Without this, any ANTHROPIC_API_KEY in the environment (e.g. from a
            // docker .env file loaded by Unity) causes claude to use API credits
            // instead of the user's Claude Code subscription.
            psi.EnvironmentVariables.Remove("ANTHROPIC_API_KEY");

            using var process = new Process { StartInfo = psi, EnableRaisingEvents = false };
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) stdout.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) stderr.AppendLine(e.Data);
            };

            if (!process.Start())
                throw new InvalidOperationException("Failed to start Claude CLI process.");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Deliver all dynamic content via stdin — no quoting needed.
            process.StandardInput.Write(stdinInput);
            process.StandardInput.Close();

            if (!process.WaitForExit(timeoutSec * 1000))
            {
                try { process.Kill(); } catch { /* ignored */ }
                throw new TimeoutException($"Claude CLI timed out after {timeoutSec} seconds.");
            }

            process.WaitForExit(); // flush async output events

            return new ClaudeCliResult
            {
                Success        = process.ExitCode == 0,
                ExitCode       = process.ExitCode,
                StandardOutput = stdout.ToString().Trim(),
                StandardError  = stderr.ToString().Trim(),
            };
        }

        // ── Argument / stdin builders ─────────────────────────────────────────────

        /// <summary>
        /// Only unquoted flags go here — no user-supplied strings.
        /// This keeps the cmd.exe argument line free of inner quotes.
        /// </summary>
        private static string BuildArguments(ClaudeEditorSettings settings)
        {
            var sb = new StringBuilder();
            sb.Append("-p --output-format text ");
            sb.Append($"--max-turns {settings.MaxTurns} ");

            if (settings.UseBypassPermissions)
                sb.Append("--permission-mode bypassPermissions ");

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Combines the system prompt and the user prompt into a single stdin payload.
        /// Replaces --append-system-prompt to avoid Windows cmd.exe quoting issues.
        /// </summary>
        private static string BuildStdin(string systemPrompt, string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(systemPrompt))
                return userPrompt;

            return systemPrompt.Trim()
                   + "\n\n"
                   + userPrompt;
        }

        // ── Path resolution ───────────────────────────────────────────────────────

        private static string ResolveClaudePath(string overridePath)
        {
            if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
                return overridePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var appData      = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                var candidates = new[]
                {
                    Path.Combine(appData,      "npm", "claude.cmd"),
                    Path.Combine(localAppData, "npm", "claude.cmd"),
                    Path.Combine(localAppData, "Programs", "claude", "claude.exe"),
                    "claude.cmd",
                    "claude.exe",
                };

                foreach (var c in candidates)
                {
                    if (Path.IsPathRooted(c) ? File.Exists(c) : true)
                        return c;
                }

                return "claude";
            }

            return "claude";
        }
    }
}
