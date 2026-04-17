---
name: Starry Night — Dream Phase & VN System
description: Dream 페이즈 진입 흐름, DreamEventRunner FSM, UIBase Show() 구독 타이밍 패턴
type: project
---

Phase 4 Dream 파트 구현 완료 (2026-03-30).

**구현 파일:**
- `Assets/PROJECT-TSN/Scripts/Common/DreamEventData.cs` — DreamNode / DreamChoice 포함 ScriptableObject
- `Assets/PROJECT-TSN/Scripts/UI/DreamKeySelectionUI.cs` — Panel_DreamKeySelection
- `Assets/PROJECT-TSN/Scripts/UI/DiceRollPopupUI.cs` — Popup_DiceRoll, autoConfirmDelay 0.5s
- `Assets/PROJECT-TSN/Scripts/UI/ChoicePopupUI.cs` — Popup_Choice, 최대 4종 동적 생성
- `Assets/PROJECT-TSN/Scripts/UI/DreamVNPanel.cs` — Panel_DreamVN, DialogueSystem 위임
- `Assets/PROJECT-TSN/Scripts/Common/DreamBaseController.cs` — Panel_Dream, Show() 에서 흐름 시작
- `Assets/PROJECT-TSN/Scripts/Common/DreamEventRunner.cs` — SingletonBase, Resources.LoadAll fallback
- `Assets/PROJECT-TSN/Scripts/Common/RoomObjects/BedObject.cs` — Dream 전환 시 UIManager.Show 추가

**중요 설계 결정:**

UIBase(프리팹 기반) 컴포넌트가 PhaseManager 이벤트를 구독하려면 OnEnable이 먼저 실행되어야 합니다. DreamBaseController는 Show() 오버라이드 안에서 VN 패널과 KeySelection을 직접 열고, BedObject가 PhaseManager.TransitionTo(Dream) 직전에 UIManager.Show<DreamBaseController>를 호출해 구독 타이밍을 보장합니다.

**Why:** UIBase 프리팹은 UIManager가 처음 Show하기 전까지 인스턴스가 없으므로, 페이즈 이벤트만 구독하면 Dream 첫 진입 시 콜백을 받지 못합니다.

**How to apply:** 페이즈 전환으로 UIBase 패널을 처음 열어야 할 때는, 전환 코드(BedObject 등)에서 UIManager.Show를 페이즈 전환 직전에 호출하는 패턴을 사용하세요.

**DreamEventRunner 완료 흐름:**
- checkEndingOnComplete && Enlightenment >= 80f → GameFlowDirector.PlayEnding()
- 그 외 → UIManager.Show(Panel_SaveScreen)

**Resources 폴더 경로 (DreamEventData SO):**
- Resources/Dream/Events/ 아래에 SO를 배치하면 Inspector 연결 없이 자동 로드됩니다.
