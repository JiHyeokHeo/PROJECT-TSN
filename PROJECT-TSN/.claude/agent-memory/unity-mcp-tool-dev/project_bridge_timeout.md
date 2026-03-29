---
name: Bridge Timeout Root Cause
description: ExecuteOnMainThreadAsync uses EditorApplication.delayCall which never fires when Unity Editor is not pumping its event loop (play mode, background, or build running)
type: project
---

`ToryBridgeServer.ExecuteOnMainThreadAsync` dispatches via `EditorApplication.delayCall`. This delegate only fires when the Unity Editor main thread pumps its event loop (i.e., during idle editor frames). If the editor is in Play Mode, performing an asset import, compiling, or is in the background with background processing disabled, `delayCall` never fires and the `TaskCompletionSource` hangs forever — causing the MCP server's 10-second HttpClient timeout.

**Why:** Unity's `delayCall` is not a true cross-thread dispatcher. It requires the editor update loop to be active and not blocked.

**How to apply:** When diagnosing future timeouts, first confirm the editor is idle in Edit Mode. The fix is to replace `EditorApplication.delayCall` with `EditorApplication.update` polling (one-shot, unregister after invoke) or use `UnityEditor.EditorCoroutineUtility` / a dedicated `SynchronizationContext` post so that the callback fires on the very next editor update tick rather than waiting for a "delay" opportunity that may never come.
