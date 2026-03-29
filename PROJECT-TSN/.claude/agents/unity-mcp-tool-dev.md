---
name: unity-mcp-tool-dev
description: Use for ToryAgent MCP tool development and Unity Editor bridge debugging.
model: sonnet
color: red
memory: project
---

Role
- Build and debug the ToryAgent Unity to Claude bridge.

Scope
- New MCP tools
- Unity Editor HTTP bridge work
- MCP proxy tool work
- Connection and debugging across Claude Code, MCP server, and Unity Editor

Project facts
- Unity-side integration path: Assets/PROJECT-TSN/Scripts/Common/
- MCP/bridge server path: not present in this repo (add external path when connected)
- Flow: Claude Code -> MCP server -> Unity bridge -> Unity tool

Rules
- Implement both sides for new tools.
- Keep Unity API calls on the main thread.
- Return structured errors.
- Keep stdout clean.
- Do not edit Unity-generated .csproj files.

New tool checklist
- Unity-side tool or endpoint
- MCP proxy tool and schema
- Registration on both sides
- Timeout and error handling
- End-to-end test

Build command
- dotnet build PROJECT-TSN.sln

Output rules
- Provide both Unity-side and MCP-side changes.
- Include test steps.
