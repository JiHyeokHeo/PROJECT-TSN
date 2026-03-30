---
name: Starry Night — Fishing & Room Interaction System
description: 우주 낚시(관측) 미니게임과 방 오브젝트 인터랙션 시스템 구조 및 구현 결정사항
type: project
---

구현된 시스템 (2026-03-29):

- `InteractableObject` (abstract, `[RequireComponent(Collider2D)]`) — MaterialPropertyBlock + `_OutlineEnabled` 셰이더 파라미터로 아웃라인 제어. OnMouseDown → OnInteract() 위임 패턴.
- `RoomObjects/` 폴더: TelescopeObject, BookshelfObject, BedObject, IlusanObject — 모두 `PhaseManager.OnPhaseChanged` static 이벤트 구독으로 IsInteractable 자동 전환.
- `Fishing/ObservationZone` — `CreateDefaultZones()` 팩토리로 3개 기본 구역 하드코딩. ScriptableObject 마이그레이션 가능 구조.
- `Fishing/ObservationRecord` — 생성자에서 displayName "Unknown [Type] #[random]" 형식 자동 할당.
- `Fishing/SpaceMapController` — MonoBehaviour (Singleton 아님). `ZoneButton` 헬퍼 컴포넌트가 인스펙터 연결 리스트로 관리됨.
- `Fishing/FishingPhaseController` — `SingletonBase<T>` 상속. `StartFishing(zone)` → `Update()` 타이머 → `EndFishing()` → `PhaseManager.TransitionTo(NightB)`.
- `Fishing/SignalPoint` — `[RequireComponent(Collider2D)]`. `Initialize(zone)` 풀 패턴 지원. `OnMouseDown` → `FocusMiniGameController.Singleton.StartMinigame(this)`.
- `Fishing/FocusMiniGameController` — `SingletonBase<T>`. `InputSystem.Singleton.OnInput_Shoot` 이벤트로 클릭 입력 수신. GreenZone 너비는 `TelescopeData.GetLevel(TelescopePart.Handle)`로 계산. rareBonus는 weights[0](Common) 일부를 weights[2/3](Rare/Legendary)으로 이전하는 방식 적용.

**Why:** UIManager.Show<UIBase>(UIList.XXX) 패턴 그대로 사용. Show/Hide 모두 UIBase 타입으로 호출 (팝업 전용 타입 없음).

**How to apply:** 새 팝업은 UIList 열거형에 UI_POPUP_START~UI_POPUP_END 사이에 추가. TelescopePart 열거형은 다른 에이전트가 작성 — Handle, Lens 등 파트명 맞춰 사용.
