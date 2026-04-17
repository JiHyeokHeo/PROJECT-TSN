---
name: 전체 화면 구현 계획 (Phase 1~4)
description: 화면 1~25 기준 Phase별 구현 순서, 담당 에이전트, 씬 구조, MainLayoutController 동기화 전략 — 2026-03-30 확정
type: project
---

## 씬 구조 결정: 단일 씬 + Additive 로드 방식

- Main.unity (항상 로드) — PhaseManager, UIManager, PlayerParameters, TelescopeData, ObservationJournal, SaveSystem
- FishingField.unity (Additive) — FishingPhaseController, VesselController 등 기존 구현
- 이유: 낚시 씬 이미 구현 완료. 나머지 화면은 UI Panel 전환으로 처리. 씬 추가 로드는 낚시 진입 시에만 필요.

## MainLayoutController 동기화 방식

- MainLayoutController는 LeftFrame(RenderTexture)과 RightFrame(UI Canvas)을 각각 독립 제어한다.
- ScreenState enum이 현재 L/R 쌍 상태를 보유하며, PhaseManager.OnPhaseChanged에 구독해 프레임 전환.
- L프레임: PhaseManager 페이즈 기반 InteractableObject Scene 전환 (GameObject SetActive)
- R프레임: UIManager.Show<T>() 호출로 Panel 전환
- L/R 동기화: MainLayoutController.SetScreen(ScreenState state)이 L + R을 동시에 전환하는 단일 진입점

---

## Phase 1 — 코어 인프라 (의존성 0순위)

목표: 저장/불러오기, 타이틀, 메뉴/옵션 팝업, MainLayoutController 기반 L/R 프레임 전환

| 화면 | 스크립트/프리팹 | 신규/기존 | 담당 에이전트 | 의존성 |
|---|---|---|---|---|
| - | SaveSystem.cs | 신규 | unity-db-ui-dev | PlayerParameters, TelescopeData, ObservationJournal (ToSaveData 미연결) |
| - | MainLayoutController.cs | 신규 | unity-content-developer | UIManager, PhaseManager |
| 22 | UI.Panel_SaveScreen (프리팹+스크립트) | 신규 | unity-db-ui-dev | SaveSystem |
| 23 | UI.Panel_LoadScreen (프리팹+스크립트) | 신규 | unity-db-ui-dev | SaveSystem |
| 24 | UI.Popup_Options (프리팹+스크립트) | 신규 | unity-db-ui-dev | - |
| 21 | UI.Popup_Menu (프리팹+스크립트) | 신규 | unity-db-ui-dev | PhaseManager 복귀 뎁스 |
| 1 | UI.Panel_Title (프리팹+스크립트) | 신규 | unity-content-developer | SaveSystem, 옵션 팝업 |

신규 UIList 항목 추가:
- Panel_Title, Panel_SaveScreen, Panel_LoadScreen
- Popup_Menu, Popup_Options

크기: Large (SaveSystem이 전체 직렬화 계층 확립)

---

## Phase 2 — 낮 사이클 (외출 루프)

목표: 방(낮), 도시, 학회/대학/상점 인터랙션 및 우측 POV/SCG 프레임

| 화면 | 스크립트/프리팹 | 신규/기존 | 담당 에이전트 | 의존성 |
|---|---|---|---|---|
| 3/14 | Panel_DayAttic (기존 UIList 항목) + DayAtticController.cs | 신규 | unity-content-developer | TelescopeObject(기존), BookshelfObject(기존), BedObject(기존), IlusanObject(기존) |
| 4/15 | Panel_DayCity + CityCameraController.cs | 신규 | unity-content-developer | PhaseManager |
| 5/16 | Panel_AcademyUI + RecordDisposalController.cs | 신규 (Popup_RecordDisposal 기존 UIList 항목 활용) | unity-content-developer | ObservationJournal.DisposePendingRecord |
| 6/17 | Panel_UniversityUI + TelescopeUpgradeController.cs | 신규 (Popup_TelescopeUpgrade 기존 UIList 항목 활용) | unity-content-developer | TelescopeData.TryUpgrade, PlayerParameters.Funds |
| 7/18 | Panel_ShopUI + DecorationShopController.cs | 신규 | unity-content-developer | PlayerParameters.Funds |
| 14,15,16,17,18 | RightFrameContentController.cs (공용) | 신규 | unity-content-developer | MainLayoutController |

크기: Medium

---

## Phase 3 — 밤 사이클 + 프롤로그/엔딩 컷신

목표: 방(밤), 낚시 진입 흐름(9/20), 컷신 시스템(2/25), 파라미터 HUD

| 화면 | 스크립트/프리팹 | 신규/기존 | 담당 에이전트 | 의존성 |
|---|---|---|---|---|
| 8/19 | Panel_NightAttic + NightAtticController.cs | 신규 | unity-content-developer | PhaseManager(NightA→Fishing), 엔딩 분기 조건 |
| 9/20 | Popup_SpaceMap (기존 UIList) + SpaceMapController(기존) | 기존 연결 | unity-content-developer | TelescopeData.GetObservableZoneCount, IlusanTimerUI |
| 20 | IlusanTimerUI.cs (Panel_Fishing 우측 프레임) | 신규 | unity-content-developer | FishingTimerUI(기존) 와 분리, IlusanObject 연동 |
| 2 | CutsceneController.cs + UI.Panel_Cutscene | 신규 | unity-content-developer | DialogueSystem(Phase 3 신규) |
| 25 | 동일 CutsceneController 재사용 | 신규 연결 | unity-content-developer | 엔딩 분기 조건 (PlayerParameters) |
| - | DialogueSystem.cs (대화/나레이션 공용) | 신규 | unity-content-developer | - |
| - | HUD_Parameters (기존 UIList 항목) 연결 | 기존 항목 신규 구현 | unity-db-ui-dev | PlayerParameters.OnParameterChanged |

크기: Large (컷신+다이얼로그 시스템이 2/12/13/25 공용)

---

## Phase 4 — 꿈 파트 + VN 시스템

목표: 꿈 이벤트 선택(11), VN 진행(12/13), 주사위 시스템, 저장 흐름 완성

| 화면 | 스크립트/프리팹 | 신규/기존 | 담당 에이전트 | 의존성 |
|---|---|---|---|---|
| 11 | Panel_DreamKeySelection.cs + UI 프리팹 | 신규 | unity-content-developer | ObservationJournal.HasCosmicTrace, CosmicTrace 아이템 목록 |
| 12 | Panel_Dream (기존 UIList) + DreamBaseController.cs | 신규 | unity-content-developer | DialogueSystem(Phase 3) |
| 13 | Panel_DreamVN.cs (우측 프레임 VN 출력) | 신규 | unity-content-developer | DialogueSystem, SCG/배경 Addressables |
| 12 | DiceRollPopup.cs + UI.Popup_DiceRoll | 신규 | unity-content-developer | DreamEventData(ScriptableObject) |
| 12 | ChoicePopup.cs + UI.Popup_Choice | 신규 | unity-content-developer | DreamEventData |
| - | DreamEventData.cs (ScriptableObject) | 신규 | game-design-architect | - |
| - | DreamEventRunner.cs (이벤트 진행 FSM) | 신규 | unity-content-developer | DreamEventData, DialogueSystem |
| 22 | SaveScreen Phase 1 구현 → 꿈 종료 후 SaveScreen으로 전환 연결 | 기존 연결 | unity-db-ui-dev | Phase 1 SaveSystem |

신규 UIList 항목 추가:
- Panel_DreamKeySelection, Panel_DreamVN
- Popup_DiceRoll, Popup_Choice

크기: Large (VN+분기+주사위 시스템)

---

## 핵심 아키텍처 결정 사항

1. 씬 분리: 단일 Main.unity + FishingField.unity Additive 로드. 씬 전환 비용 없이 UI Panel SetActive로 화면 전환.

2. MainLayoutController 역할: ScreenState(enum) 기반으로 L프레임(RenderTexture Camera 대상 전환) + R프레임(UIManager.Show/Hide) 동시 제어. PhaseManager.OnPhaseChanged 구독으로 자동 동기화.

3. DialogueSystem: 컷신(2/25), 우측 POV 나레이션(16/17/18/20), 꿈 VN(13) 모두 공용 사용. 텍스트 자산은 ScriptableObject 기반 StoryData로 관리.

4. SaveSystem: PlayerParameters, TelescopeData, ObservationJournal, PhaseManager(Day), DreamEventRunner(진행도)의 ToSaveData() 집약. FileManager 경유 JSON 저장. 슬롯 3개.

5. 옵션/메뉴 복귀 뎁스: Popup_Options와 Popup_Menu는 열릴 때 이전 ScreenState를 스택에 push, 닫힐 때 pop으로 복귀. 별도 씬 전환 불필요.

6. 엔딩 분기: NightAtticController가 PlayerParameters 값 조건 체크 후 … 선택지 활성화. 25번 화면은 CutsceneController 재사용.

---

**Why:** 2026-03-30 사용자가 화면 1~25 전체 명세를 제공하며 Phase별 구현 계획 수립 요청.
**How to apply:** 이후 각 Phase 구현 의뢰 시 여기서 정의한 스크립트명/UIList 항목명/에이전트 배정을 기준으로 삼을 것.
