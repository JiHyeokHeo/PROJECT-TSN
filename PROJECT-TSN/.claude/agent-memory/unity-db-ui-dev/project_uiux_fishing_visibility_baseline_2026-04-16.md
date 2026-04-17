# UI/UX + Fishing Spec Baseline (2026-04-16)

Source documents:
- `C:/Users/HEOJIHYEOK/Downloads/_UIUX 레파토리 기획서.pdf`
- `C:/Users/HEOJIHYEOK/Downloads/낚시 게임 파트 기획서.pdf`

Core baseline decisions:
- Keep the left/right two-panel main layout persistent across phase transitions.
- Fishing phase timer base duration is 300 seconds.
- During fishing, timer should not progress while blocking overlays are open.
- Blocking overlays include at least:
  - Menu popup
  - Inventory popup
  - Observation record/journal popup
  - Skip warning popup (if present)

Implementation note:
- Prefer shared `UIManager` visibility events for cross-UI reactions instead of hard-coding UI-to-UI direct calls inside popup classes.
