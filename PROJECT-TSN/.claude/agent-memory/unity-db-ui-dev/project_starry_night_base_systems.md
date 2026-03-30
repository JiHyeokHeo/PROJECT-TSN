---
name: Starry Night Base Systems
description: Core systems implemented for The Starry Night — player parameters, observation journal, telescope, phase manager, and UI bindings
type: project
---

Core systems scaffolded for The Starry Night (PROJECT-TSN), a cosmic horror space-observation game.

Files created:
- `Assets/PROJECT-TSN/Scripts/Common/PlayerParameters.cs` — SingletonBase, Fame/Sanity/Enlightenment/Madness (0~100), Funds (double), OnParameterChanged event
- `Assets/PROJECT-TSN/Scripts/Common/ObservationRecord.cs` — RecordType/Rarity enums, ObservationRecord class, ObservationJournal singleton, DisposalMethod enum + DisposeResult, DisposalTable (delta calc by rarity)
- `Assets/PROJECT-TSN/Scripts/Common/TelescopeData.cs` — SingletonBase, TelescopePartType enum (7 parts), lv1~5, TryUpgrade deducts funds, GetRareBonus per RecordType, GetObservableZoneCount from Lens level
- `Assets/PROJECT-TSN/Scripts/Common/PhaseManager.cs` — SingletonBase, GamePhase enum (DayAttic/DayCity/NightA/Fishing/NightB/Dream), CurrentDay increments on DayAttic re-entry from night-side, OnPhaseChanged event
- `Assets/PROJECT-TSN/Scripts/UI/HUD_Parameters.cs` — UIBase, subscribes to PlayerParameters.OnParameterChanged via OnEnable/OnDisable, TMP labels for 4 params + funds
- `Assets/PROJECT-TSN/Scripts/UI/MainLayoutController.cs` — MonoBehaviour, left (circular mask) / right (rect) frames, SetLeftContent/SetRightContent overloads, ShowDialogue/HideDialogue, delegates phase changes via OnPhaseContentRequested event

UIList.cs updated with Starry Night panels and popups between sentinel values.

**Why:** New game replacing idle garden game. Old GameManager/UserDataModel/GameDataModel are commented out and must not be reused.
**How to apply:** All new game systems are standalone singletons. Do not couple to old Idle game classes. Persistence via FileManager.WriteFileFromString/ReadFileData — calls left as TODO stubs in each Save()/Load() method.
