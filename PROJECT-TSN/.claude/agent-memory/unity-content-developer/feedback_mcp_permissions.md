---
name: MCP tool permissions — create_material and create_folder denied
description: mcp__toryagent__create_material and create_folder are not permitted; use editor scripts instead
type: feedback
---

Do not attempt to use mcp__toryagent__create_material or mcp__toryagent__create_folder.

**Why:** Both tools returned permission-denied errors during the idle building asset session.
**How to apply:** For material and folder creation tasks, write an [InitializeOnLoad] editor script that uses AssetDatabase / Directory.CreateDirectory / new Material(shader) directly. This is also more portable and reproducible than MCP calls.
