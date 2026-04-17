---
name: Phase 1 Core Infrastructure — Save System and Title/Menu UI
description: SaveSystem, UIList updates, and all Phase 1 UI files are fully implemented. Notes on what is complete and one integration fix applied.
type: project
---

All Phase 1 deliverables were already implemented when checked on 2026-03-30. No new files were written.

**Completed files (all in Assets/PROJECT-TSN/Scripts/):**
- `Common/UIList.cs` — Panel_Title, Panel_Cutscene, Panel_SaveScreen, Panel_LoadScreen, Panel_DreamKeySelection, Panel_DreamVN added to Panel section; Popup_Menu, Popup_Options, Popup_DiceRoll, Popup_Choice, Popup_NightTelescope added to Popup section.
- `Common/SaveSystem.cs` — SingletonBase<SaveSystem>, SLOT_COUNT=3, Save/Load/HasSave/HasAnySave/GetPreview/DeleteSave. Uses `double` for funds (correct; matches PlayerParameters).
- `UI/TitleUI.cs` — UIBase, Panel_Title. Buttons: new game (TransitionTo DayAttic), load (Panel_LoadScreen), options (Popup_Options fromTitle:true), quit. RefreshLoadButton via HasAnySave().
- `UI/SaveScreenUI.cs` — UIBase, Panel_SaveScreen. 3 slots + optional closeButton. Labels: "Day N | date" or "빈 슬롯".
- `UI/LoadScreenUI.cs` — UIBase, Panel_LoadScreen. 3 slots, interactable=HasSave. On success shows MainLayout.
- `UI/MenuPopupUI.cs` — UIBase, Popup_Menu. ESC toggle via _escEnabled flag. SetEscEnabled(bool) public.
- `UI/OptionsPopupUI.cs` — UIBase, Popup_Options. Show(bool fromTitle). BGM/SFX via PlayerPrefs. OnClose returns to TitleUI or MenuPopupUI.

**Fix applied 2026-03-30:**
TitleUI.Show() and Hide() now call MenuPopupUI.SetEscEnabled(false/true) via UIManager.GetUI<>() so ESC cannot open the in-game menu from the title screen.

**Why:** MenuPopupUI relies on _escEnabled flag; TitleUI must toggle it on Show/Hide because there is no phase-check inside MenuPopupUI itself.
