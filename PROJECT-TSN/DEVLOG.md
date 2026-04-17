# DEVLOG — The Starry Night

---

## [2026-04-13] ItemDefinitionSO 생성 · GameDataModel 복원 · InventoryPopupUI 아이콘 연동

### 작업 1: ItemDefinitionSO 클래스 신규 생성
- 경로: `Assets/PROJECT-TSN/Scripts/Common/ItemDefinitionSO.cs`
- `[CreateAssetMenu(menuName = "TST/Item Definition")]` — 에셋 생성 메뉴 등록
- 필드: `itemId (string)`, `itemName (string)`, `recordType (RecordType)`, `rarity (Rarity)`, `description (string)`, `icon (Sprite)`
- `RecordType`, `Rarity`는 `ObservationRecord.cs`에 정의된 enum 재사용 (중복 선언 없음)
- `ObservationRecord.id`를 `itemId`와 동일하게 사용하는 방침 확정 (별도 필드 추가 없음, JSON 호환 유지)

### 작업 2: GameDataModel 복원 및 캐싱 로직 구현
- 경로: `Assets/PROJECT-TSN/Scripts/Common/GameDataModel.cs`
- 기존 전체 주석 코드를 해제하고 실동작 코드로 교체
- `List<ItemDefinitionSO> ItemDatas` — Inspector 할당
- `Dictionary<string, ItemDefinitionSO> _itemById` — Awake 시 자동 Initialize
- 중복 itemId 경고 로그 추가
- `GetItemData(string itemId, out ItemDefinitionSO resultData)` 공개 메서드
- `_itemById`가 null이거나 비어 있으면 GetItemData() 최초 호출 시 Initialize() 재실행 (방어 처리)

### 작업 3: InventoryPopupUI 상세 패널 아이콘 연동
- 경로: `Assets/PROJECT-TSN/Scripts/UI/InventoryPopupUI.cs`
- `ShowDetail()` 수정: `GameDataModel.Singleton.GetItemData(record.id, out def)` 조회
- `def.icon != null` 이면 `detailIconImage.sprite = def.icon`, `color = Color.white`
- SO가 없거나 icon이 null이면 기존 `GetRarityColor(rarity)` fallback 유지

### Inspector 연결 필요 사항
- `GameDataModel` GameObject Inspector → `ItemDatas` 리스트에 `Assets/PROJECT-TSN/Data/Items/` 아래 생성한 SO 에셋 할당
- ItemDefinitionSO 에셋 생성: 우클릭 → Create → TST → Item Definition, `itemId`를 `ObservationRecord.id`와 동일하게 입력

---

## [2026-04-02] 싱글톤 리팩터링 · 폴더 구조 · Inspector 연결 · NightTelescopePopupUI 연동

### 작업 1: Space→Fishing 전환 시 FishingHUD 관리 구조 확립
- `FishingTransitionController.BeginFishingTransition(ObservationZone zone)` — zone 파라미터 추가
- `FishingTransitionRoutine` 페이드인 완료 후 `FishingPhaseController.StartFishing(zone)` 호출 (HUD가 검은 화면 중 뜨지 않도록 타이밍 확정)
- `FishingHudUI`에 `static Singleton` 추가 (씬 내 오브젝트라 UIManager 경유 불가, Hazard에서 직접 접근)
- `FishingPhaseController`에 `[SerializeField] FishingHudUI fishingHudUI` 추가 → `StartFishing()`에서 Show, `EndFishing()`에서 Hide
- `Hazard.cs` `FindFirstObjectByType` → `FishingHudUI.Singleton?.ShowWarningLight()` 교체

### 작업 2: 씬 종속 클래스 싱글톤 전면 제거 (5개 → MonoBehaviour)
제거 대상: `FishingPhaseController`, `VesselController`, `VesselHull`, `SpaceMapController`, `FocusMiniGameController`, `FishingTransitionController`

- 모든 `.Singleton` 참조를 `[SerializeField]` Inspector 필드로 교체
- `FishingPhaseController.ShouldTickTimer()` — UIBase 4종 Awake 시 캐싱으로 교체 (매 프레임 GetUI 호출 제거)
- `FocusMiniGameController` `Input.GetMouseButtonDown/GetKeyDown` → New Input System(`Mouse.current`, `Keyboard.current`) 교체
- 유지 싱글톤: `PhaseManager`, `SaveSystem`, `SoundManager`, `InputSystem`, `UIManager`, `PlayerParameters`

### 작업 3: Inspector 파라미터 연결 (MCP)
씬 내 28개 필드 MCP toryagent로 연결:
FishingPhaseController, FishingTransitionController, SpaceMapController, VesselHull, FishingHudUI, Hazard×2, ActivityBoundary, VesselSpriteController, VesselDirectionPointer, ParallaxLayerController, FocusMinigameUI, FishingGround×3, RecordArchive×2

미연결 (씬 오브젝트 특정 불가, 수동 필요): `FishingTransitionController`의 `globalVolume`, `spaceBG`, `gridGround`, `playerCamera`, `transitionCamera2D`

### 작업 4: 폴더 구조 정리
- `Common/Controllers/` 신규 생성 → 페이즈 컨트롤러 5개 이동 (DayAttic, NightAttic, DayCity, DreamBase, RightFrameContent)
- `FishingTransitionController` → `Common/Fishing/`으로 이동
- `.meta` 파일 함께 이동 (GUID 보존, 씬 참조 유지)

### 작업 5: 씬 구조 판단 (단일 씬 유지)
Inspector 직렬화 패턴이 cross-scene 참조를 지원하지 않아 씬 분리 시 다시 싱글톤으로 회귀하게 됨 → 단일 씬 유지 결정

### 작업 6: NightTelescopePopupUI 컨트롤러 연동
- UIManager가 Resources에서 인스턴스화하는 popup은 씬 오브젝트 참조를 prefab에 연결 불가
- **Inject-on-Show 패턴** 적용: `UIManager.Show<NightTelescopePopupUI>()?.Initialize(fishingTransitionController)`
- `TelescopeObject`에 `[SerializeField] FishingTransitionController` 추가, NightA/NightB 케이스 통합
- `NightTelescopePopupUI.Initialize(FishingTransitionController)` 메서드 추가
- Day/Night Telescope GameObject 각각 MCP로 연결 완료

---

## [2026-04-01] 관측선 침몰 패널티 — 이성/광기 스탯 연동 및 미보관 기록 소실

### 작업 1: `ObservationJournal.ClearHeldRecords()` 추가 (`ObservationRecord.cs`)
- 기존 `ClearPendingRecords()`는 `_pendingRecords`만 비우고 `_allRecords`는 유지 (처리 완료 기록 보존 목적)
- `ClearHeldRecords()` 신규 메서드 추가: `_pendingRecords`의 항목을 `_allRecords`에서도 제거 후 목록 클리어
- 이미 처리(보관)된 기록(`_allRecords`에만 남은 항목)은 영향 없음

### 작업 2: `VesselHull.HandleSunk()` 침몰 패널티 처리 추가 (`VesselHull.cs`)
- `EndFishing()` 호출 전에 세 가지 패널티를 순서대로 실행
  1. `ObservationJournal.Singleton.ClearHeldRecords()` — 미보관 기록 전부 소실
  2. `Mathf.FloorToInt(pp.Sanity * 0.1f)` → `pp.AddSanity(-penalty)` — 이성 현재값 10% 감소 (소수점 버림)
  3. `Mathf.FloorToInt(pp.Madness * 0.1f)` → `pp.AddMadness(+penalty)` — 광기 현재값 10% 상승 (소수점 버림)
- `PlayerParameters`에 null 체크 적용, 계산값 0인 경우 Add 호출 생략
- 스탯 변경은 기존 `PlayerParameters.AddSanity() / AddMadness()` 경로를 사용하므로 `OnParameterChanged` 이벤트 자동 발행

### 데이터 흐름 정리
- `PlayerParameters.Singleton` — 이성(`Sanity`), 광기(`Madness`) 스탯 보유 및 변경 API 제공
- `ObservationJournal.Singleton` — `_pendingRecords`: 미처리(미보관), `_allRecords`: 전체(보관 포함)
- 침몰 패널티는 `EndFishing()` 이전에 적용되어 페이즈 종료 후 UI 동기화 자연스럽게 처리됨

### Inspector 와이어링 필요 없음
- 모든 참조는 싱글톤 자동 접근

---

## [2026-04-01] 낚시 파트 기획서 반영 — 6개 항목 구현

### 작업 1: 타이머 소모 조건 변경 (`FishingPhaseController.cs`)
- `IsActive` 일 때 항상 감소 → 조건부 감소로 변경
- 감소 조건: `VesselController.SpeedRatio > 0` (이동 중) 또는 `FocusMiniGameController.State == Active` (미니게임 중)
- 정지 조건 (우선): `Popup_Menu` 열림, `IsSkipConfirmShowing == true`, `Popup_Inventory` 열림, `Popup_ObservationJournal` 열림
- `IsSkipConfirmShowing` 공개 프로퍼티 추가 (SkipFishing 경고 팝업 표시 시 호출자가 설정)

### 작업 2: 조타륜 나침반 레이어2 (`FishingHudUI.cs`)
- `[SerializeField] Image compassImage` Inspector 필드 추가
- `Update()`에서 관측선 위치 → RecordArchive 위치 방향각 계산 후 compassImage 회전
- `Show()`에서 `FindFirstObjectByType<RecordArchive>()` 캐시
- RecordArchive 없으면 compassImage 회전 정지

### 작업 3: 미니게임 M2/Esc 취소 (`FocusMiniGameController.cs`)
- `CancelMinigame()` 공개 메서드 추가
- `Update()`에서 `Input.GetMouseButtonDown(1)` 또는 `Input.GetKeyDown(KeyCode.Escape)` 감지 → `CancelMinigame()` 호출
- 취소 시: State → Idle, UI 닫기, `OnMiniGameCompleted` 이벤트 미발행, FishingGround 유지

### 작업 4: Hazard 소실/리스폰 시스템 (`Hazard.cs`)
- `[SerializeField] float despawnDelay = 3f`, `respawnDelay = 10f` 추가
- 최초 접촉 플래그(`_firstContact`) 추가
- 최초 접촉 시 `DespawnRoutine()` 코루틴 시작 → despawnDelay 후 `gameObject.SetActive(false)`
- `SetActive(false)` 전에 `FishingPhaseController.StartCoroutine(RespawnRoutine(...))` 위임
- `OnEnable()`에서 상태 초기화 (리스폰 후 재사용 대비)

### 작업 5: 카메라 쉐이킹 + 경고등 UI
- `VesselCameraController`: `static Singleton` 프로퍼티 추가, `ShakeCamera(float duration, float magnitude = 0.3f)` 코루틴 추가
- 쉐이킹: `_shakeOffset = Random.insideUnitSphere * magnitude`, LateUpdate에서 위치 오프셋 적용
- `FishingHudUI`: `[SerializeField] GameObject warningLight` 추가, `ShowWarningLight()` (1.5초 코루틴) 추가
- `Hazard`: 최초 접촉 시 `VesselCameraController.Singleton.ShakeCamera(0.5f)` + `FindFirstObjectByType<FishingHudUI>().ShowWarningLight()` 호출

### 작업 6: RecordArchive 100% 회복 (`RecordArchive.cs`)
- `hull.Recover(recoverAmount)` → `hull.Recover(hull.MaxDurability)` 로 변경
- `recoverAmount` 기본값 30f → 100f (Inspector 오버라이드 유지, 실제 로직에서는 미사용)

### Inspector 와이어링 필요
- `FishingHudUI`: `compassImage` (조타륜 레이어2 Image), `warningLight` (경고등 GameObject) 연결
- `VesselCameraController`: 별도 Inspector 연결 없음 (Singleton 자동 등록)

---

## [2026-03-30] MCP 씬 배치 — BootStrapper / SoundManager / GameManager

### 배치 결과

| 오브젝트 | 결과 |
|---|---|
| `BootStrapper` | 기존 존재, TST.BootStrapper 컴포넌트 확인 |
| `SoundManager` | 신규 생성, TST.SoundManager 컴포넌트 추가 |
| `GameManager` | 신규 생성, TST.GameManager 컴포넌트 추가 |

### SO 에셋 존재 확인
- Story_Prologue.asset ✅
- Story_Ending_Enlightenment.asset ✅
- DreamEvent_Dialogue/Choice/Dice_Sample.asset ✅ (3개 모두)

### Inspector 수동 연결 필요 (MCP 도구 SO 레퍼런스 미지원)

**GameFlowDirector** (`--- Managers ---` > GameFlowDirector):
- `Prologue Data` → `Story_Prologue`
- `Ending Data` → `Story_Ending_Enlightenment`

**DreamEventRunner** (`--- Managers ---` > DreamEventRunner):
- `Available Events[0]` → `DreamEvent_Dialogue_Sample`
- `Available Events[1]` → `DreamEvent_Choice_Sample`
- `Available Events[2]` → `DreamEvent_Dice_Sample`
- ⚠️ SO가 Resources 경로 밖에 있으므로 Inspector 연결 필수

### Script Execution Order 수동 설정 필요
```
Edit > Project Settings > Script Execution Order
  + BootStrapper → 100
```

---

## [2026-03-30] 나머지 작업 — 미구현 스크립트 + CreateUIPrefabs 완성

### 신규 스크립트 2개

#### `Scripts/UI/ObservationJournalPopupUI.cs` (Popup_ObservationJournal)
- ObservationJournal의 전체 기록 열람 팝업
- 필터 탭 4종 (전체 / CelestialBody / Phenomenon / CosmicTrace)
- 항목 클릭 시 detailPanel에 이름·등급·설명 표시
- Inspector: recordListContainer, recordItemPrefab, filterButtons[4], detailPanel 3종 라벨, emptyLabel, closeButton

#### `Scripts/UI/InventoryPopupUI.cs` (Popup_Inventory)
- ObservationJournal의 pendingRecords(미처분 기록) 확인 팝업
- 처분은 Popup_RecordDisposal(AcademyController)에서 진행
- Inspector: recordListContainer, recordItemPrefab, detailPanel 4종 라벨, emptyLabel, closeButton

### CreateUIPrefabs.cs — 나머지 UIBase 항목 추가
추가된 항목:
- MainLayout(50) → MainLayoutController
- HUD_Parameters(60) → HUD_Parameters
- Panel_FishingHUD → FishingHudUI
- Panel_FishingTimer → FishingTimerUI
- Popup_Inventory → InventoryPopupUI
- Popup_ObservationJournal → ObservationJournalPopupUI
- Popup_TelescopeUpgrade → UniversityController
- Popup_RecordDisposal → AcademyController
- Popup_FocusMinigame → FocusMinigameUI
- Popup_DecorationShop → DecorationShopController

### UIList 커버리지 (UIBase 기준)
| 항목 | 상태 |
|---|---|
| MainLayout | ✅ CreateUIPrefabs |
| HUD_Parameters | ✅ CreateUIPrefabs |
| Panel_DayAttic~Dream | SingletonBase (씬 배치) |
| Panel_Fishing | SingletonBase (씬 배치) |
| Panel_FishingHUD/Timer | ✅ CreateUIPrefabs |
| Panel_Title~DreamVN | ✅ CreateUIPrefabs |
| Popup_Inventory | ✅ CreateUIPrefabs |
| Popup_ObservationJournal | ✅ CreateUIPrefabs |
| Popup_TelescopeUpgrade | ✅ CreateUIPrefabs |
| Popup_RecordDisposal | ✅ CreateUIPrefabs |
| Popup_SpaceMap | ⚠️ MonoBehaviour (별도 처리 필요) |
| Popup_FocusMinigame | ✅ CreateUIPrefabs |
| Popup_DecorationShop | ✅ CreateUIPrefabs |
| Popup_Menu/Options/Dice/Choice/NightTelescope | ✅ CreateUIPrefabs |

### Unity에서 실행
1. `Tools > TST > Create Story Data Assets`
2. `Tools > TST > Create Dream Event Assets`
3. `Tools > TST > Create UI Prefabs` (전체 21개 프리팹 생성)

---

## [2026-03-30] SaveLoad 시스템 구축 — SoundManager 재작성 / Phase 복원 / 자동저장

### 작업 배경
SoundManager가 ClockStone 서드파티 라이브러리 의존으로 전체 주석 처리 상태였음.
SaveData에 currentPhase 누락, PhaseManager에 ForceSetPhase 없음, BootStrapper 미구현 상태.

### 변경 파일

#### `SoundManager.cs` — 완전 재작성
- ClockStone 의존성 제거, Unity 네이티브 AudioSource 3채널(BGM/SFX/UI)로 재구현
- `Volume_Master / Volume_BGM / Volume_SFX / Volume_UI` 프로퍼티 — PlayerPrefs 영속 + AudioSource 즉시 적용
- AudioSource 미연결 시 Awake에서 자식 GameObject 자동 생성
- `PlayBGM(clip)`, `StopBGM()`, `PlaySFX(clip)`, `PlayUISFX(clip)`, `StopAll()`
- `Initialize()` — 저장된 볼륨 복원 (BootStrapper에서 호출)
- `SaveVolumes()` — PlayerPrefs.Save() 래퍼

#### `PhaseManager.cs` — 메서드 2개 추가
- `ForceSetPhase(GamePhase)` — 이벤트 미발동, 세이브 복원 전용
- `ForceTransitionTo(GamePhase)` — 이벤트 발동, 로드 직후 UI 동기화 전용

#### `SaveSystem.cs` — currentPhase + LastUsedSlot 추가
- `SaveData.currentPhase` (int) 필드 추가
- `BuildSaveData`: `pm.CurrentPhase` → `currentPhase` 저장
- `ApplySaveData`: `ForceSetPhase((GamePhase)data.currentPhase)` 복원
- `LastUsedSlot` 프로퍼티 — Save/Load 호출 시 갱신, 자동저장 기준

#### `OptionsPopupUI.cs` — SoundManager 연결
- `AudioListener.volume` 임시 proxy 제거
- `OnBGMChanged` → `SoundManager.Singleton.Volume_BGM = value`
- `OnSFXChanged` → `SoundManager.Singleton.Volume_SFX = value`
- `LoadSettings` → SoundManager에서 볼륨값 읽어 슬라이더 초기화

#### `LoadScreenUI.cs` — 로드 후 UI 동기화
- 슬롯 클릭 후: `PhaseManager.Singleton.ForceTransitionTo(CurrentPhase)` 호출
- → MainLayout 및 RightFrame 등 모든 Phase 구독자가 즉시 갱신됨

#### `GameManager.cs` — 자동 저장
- `OnApplicationPause(true)` → AutoSave
- `OnApplicationQuit()` → AutoSave + PlayerPrefs.Save
- AutoSave: `SaveSystem.LastUsedSlot >= 0`인 경우에만 해당 슬롯에 저장 (첫 플레이 세션 스킵)

#### `BootStrapper.cs` — TSN용 재작성
- UIManager.Initialize() → SoundManager.Initialize() → Panel_Title 표시
- Script Execution Order를 Default보다 늦게(예: +100) 설정 권장

### 초기화 흐름
```
씬 로드
  → BootStrapper.Start()
      → UIManager.Initialize()    (Panel Root / Popup Root / UICamera)
      → SoundManager.Initialize() (볼륨 복원)
      → UIManager.Show<TitleUI>() (타이틀 화면)
          → 새 게임: GameFlowDirector.PlayPrologue() → DayAttic
          → 불러오기: LoadScreenUI → SaveSystem.Load() → ForceTransitionTo() → MainLayout
          → 옵션: OptionsPopupUI (fromTitle=true)
```

### 남은 작업
- BootStrapper → Script Execution Order 설정 (Unity Project Settings)
- SoundManager Inspector에 AudioSource 연결 (또는 자동 생성 사용)
- 각 Phase 전환 시 BGM 클립 연결 (SoundManager.PlayBGM 호출)

---

## [2026-03-30] SO 에셋 Editor 스크립트 + UI 프리팹 Editor 스크립트 추가

### 작업 내용

#### DreamEventData SO 에셋 생성기
- `Scripts/Editor/CreateDreamEventAssets.cs` 추가
- 메뉴: `Tools > TST > Create Dream Event Assets`
- 저장 경로: `Assets/PROJECT-TSN/ScriptableObjects/Dream/Events/`
- 생성 에셋 3종:
  - `DreamEvent_Dialogue_Sample.asset` — 기본 대화 흐름 (Dialogue → End)
  - `DreamEvent_Choice_Sample.asset` — 분기 선택지 (Dialogue → Choice → 결과A/B → End), 선택지B 계몽 30 이상 조건
  - `DreamEvent_Dice_Sample.asset` — 주사위 판정 (Dialogue → Dice [4이상 성공, 되감기 1회] → 결과 → End)
- `triggerRecordType` 별 각 RecordType 커버 (CosmicTrace / CelestialBody / Phenomenon)

#### StoryData SO 에셋 생성기 (기존)
- `Scripts/Editor/CreateStoryDataAssets.cs` 이미 존재 — 실행하면 됨
- 메뉴: `Tools > TST > Create Story Data Assets`
- 저장 경로: `Assets/PROJECT-TSN/ScriptableObjects/Story/`
- Story_Prologue / Story_Ending_Enlightenment / Story_DreamIntro

#### UI 프리팹 생성기
- `Scripts/Editor/CreateUIPrefabs.cs` 추가
- 메뉴: `Tools > TST > Create UI Prefabs`
- 저장 경로: `Assets/Resources/UI/Prefabs/` (UIManager.cs `Resources.Load` 경로 준수)
- Canvas(1920×1080 ScaleWithScreenSize) + Panel(투명 전체화면) 구조

| 프리팹 | 컴포넌트 |
|---|---|
| `UI.Panel_Title` | `TitleUI` |
| `UI.Panel_Cutscene` | `CutsceneController` |
| `UI.Panel_SaveScreen` | `SaveScreenUI` |
| `UI.Panel_LoadScreen` | `LoadScreenUI` |
| `UI.Panel_DreamKeySelection` | `DreamKeySelectionUI` |
| `UI.Panel_DreamVN` | `DreamVNPanel` |
| `UI.Popup_Menu` | `MenuPopupUI` |
| `UI.Popup_Options` | `OptionsPopupUI` |
| `UI.Popup_DiceRoll` | `DiceRollPopupUI` |
| `UI.Popup_Choice` | `ChoicePopupUI` |
| `UI.Popup_NightTelescope` | `NightTelescopePopupUI` |

### Unity 실행 순서
1. `Tools > TST > Create Story Data Assets`
2. `Tools > TST > Create Dream Event Assets`
3. `Tools > TST > Create UI Prefabs`

### 남은 Inspector 연결 작업
- 각 UI 프리팹 내 SerializeField 필드 수동 연결 (버튼, 텍스트, 슬라이더 등)
- DreamEventData dialogue 필드에 Story SO 에셋 연결
- GameFlowDirector의 prologueData / endingData 에 Story SO 연결
- DreamEventRunner의 availableEvents 배열에 DreamEvent SO 연결

---

## [2026-03-30] MCP 씬 배치 — 방/도시 인터랙션 오브젝트 배치 및 연결

### 작업 배경

DayAtticController / NightAtticController / DayCityController에 연결할 방·도시 인터랙션 오브젝트를 MCP로 씬에 배치했다.

### 배치 결과

```
--- Room ---
  └── AtticObjects
        ├── Telescope     [BoxCollider2D, TelescopeObject]   (-3, 0, 0)
        ├── Bookshelf     [BoxCollider2D, BookshelfObject]   ( 0, 0, 0)
        ├── Bed           [BoxCollider2D, BedObject]         ( 3, 0, 0)
        ├── Door          [BoxCollider2D, DoorObject]        ( 5, 0, 0)
        └── Ilusan        [BoxCollider2D, IlusanObject]      (-5, 0, 0)

CityObjects
  ├── AcademyBtn    [Button]
  ├── UniversityBtn [Button]
  ├── ShopBtn       [Button]
  └── HomeBtn       [Button]
```

### 컨트롤러 필드 연결 완료

| 컨트롤러 | 필드 | 연결 오브젝트 |
|---|---|---|
| `DayAtticController` | `telescopeObj` | Telescope |
| `DayAtticController` | `bookshelfObj` | Bookshelf |
| `DayAtticController` | `bedObj` | Bed |
| `DayAtticController` | `doorObj` | Door |
| `NightAtticController` | `bookshelfObject` | Bookshelf |
| `DayCityController` | `academyBtn` | AcademyBtn |
| `DayCityController` | `universityBtn` | UniversityBtn |
| `DayCityController` | `shopBtn` | ShopBtn |
| `DayCityController` | `homeBtn` | HomeBtn |

### 추가 처리
- ObservationJournal 싱글톤 씬 누락 → `--- Managers ---` 하위에 컴포넌트 추가

### 남은 작업
- 각 오브젝트에 실제 스프라이트/비주얼 추가
- `StoryData SO` 제작 (프롤로그/엔딩)
- `DreamEventData SO` 제작 (꿈 이벤트)
- UI 프리팹 제작 (Panel_Title, Popup_Menu 등)

---

## [2026-03-30] MCP 씬 배치 — 매니저/컨트롤러 오브젝트 생성

### 작업 배경

Phase 1~4 스크립트 구현 완료 후 MCP ToryAgent로 씬에 오브젝트를 배치했다.

### 배치 결과

```
--- Managers ---
  ├── GameFlowDirector              TST.GameFlowDirector
  ├── DreamEventRunner              TST.DreamEventRunner
  ├── RightFrameContentController   TST.RightFrameContentController
  ├── DialogueSystem                TST.DialogueSystem
  └── SaveSystem                    TST.SaveSystem

--- Controllers ---
  ├── DayAtticController            TST.DayAtticController
  ├── DayCityController             TST.DayCityController
  ├── NightAtticController          TST.NightAtticController
  └── DreamBaseController           TST.DreamBaseController
```

### 남은 Inspector 연결 작업

씬에 TelescopeObject / BookshelfObject / BedObject 오브젝트가 없어 연결 보류.
해당 오브젝트 배치 후:

| 컴포넌트 | 필드 | 연결 대상 |
|---|---|---|
| `DayAtticController` | `telescopeObj` | TelescopeObject |
| `DayAtticController` | `bookshelfObj` | BookshelfObject |
| `DayAtticController` | `bedObj` | BedObject |
| `NightAtticController` | `bookshelfObject` | BookshelfObject |
| `GameFlowDirector` | `prologueData` / `endingData` | StoryData SO |
| `DreamEventRunner` | `availableEvents` | DreamEventData SO 배열 |
| `RightFrameContentController` | Phase별 배경 Sprite 6종 | — |

---

## [2026-03-30] 전체 화면 흐름 명세 기반 게임 루프 전체 구현

### 작업 배경

화면 1~25 전체 명세서를 기준으로 Phase 1~4로 나눠 전체 게임 루프를 구현했다.

### Phase 1 — 코어 인프라

**`UIList.cs` 업데이트**
- Panel: `Panel_Title`, `Panel_Cutscene`, `Panel_SaveScreen`, `Panel_LoadScreen`, `Panel_DreamKeySelection`, `Panel_DreamVN`
- Popup: `Popup_Menu`, `Popup_Options`, `Popup_DiceRoll`, `Popup_Choice`, `Popup_NightTelescope`, `Popup_DecorationShop`

**`SaveSystem.cs`** (신규)
- `SingletonBase<SaveSystem>`, 슬롯 3개, JSON, `Application.persistentDataPath`
- `Save(int)` / `Load(int)` / `HasSave(int)` / `GetPreview(int)` / `DeleteSave(int)`
- PlayerParameters / TelescopeData / ObservationJournal / PhaseManager.CurrentDay 직렬화

**신규 UI 스크립트**: `TitleUI`, `SaveScreenUI`, `LoadScreenUI`, `MenuPopupUI`, `OptionsPopupUI`

---

### Phase 2 — 낮 루프

**`RightFrameContentController.cs`** (신규)
- Phase 전환 시 우측 프레임 배경 Sprite 자동 교체
- `SetSubLocation(SubLocation)` — 학회/대학/상점 NPC 초상화 + 나레이션 표시

**`DayAtticController.cs`** (신규)
- 망원경 → `Popup_TelescopeUpgrade` / 책장 → `Popup_ObservationJournal`
- 침대 → `NightA` / 문 → `DayCity`

**`DayCityController.cs`** (신규)
- 학회/대학/상점/집 버튼 → 서브 로케이션 패널 전환 또는 Phase 전환
- 학회/대학/상점은 DayCity Phase 내 UI 전환만 (새 Phase 없음)

**`AcademyController.cs`** (`Popup_RecordDisposal`)
- 처분 5종: 발표(Fame+Funds) / 기록(Sanity) / 탐구(Enlightenment) / #@!$%(Madness) / 돌아간다
- 레코드 없으면 처분 버튼 비활성

**`UniversityController.cs`** (`Popup_TelescopeUpgrade`) (신규)
- 파츠 7종 행 동적 생성, 레벨별 비용 테이블, Funds 부족/최대 레벨 시 버튼 비활성

**`DecorationShopController.cs`** (`Popup_DecorationShop`) (신규)
- 장식품 플레이스홀더 3종, Funds 차감, 부족 시 비활성

---

### Phase 3 — 밤 루프 + 컷신

**`StoryData.cs`** (ScriptableObject 신규)
- `DialogueLine[]`: speakerName, text, portrait, backgroundCg
- `nextPhase`, `goToTitle` 전환 필드

**`DialogueSystem.cs`** (신규)
- 코루틴 타이핑 (typingSpeed = 0.03f)
- `PlayStory(StoryData, Action)` / `Skip()` 3단계 분기

**`CutsceneController.cs`** (`Panel_Cutscene`) (신규)
- 페이드인/아웃 + DialogueSystem 연동
- 건너뛰기 버튼 지원

**`GameFlowDirector.cs`** (신규)
- `PlayPrologue()` / `PlayEnding()` — 프롤로그/엔딩 컷신 분기 일원화

**`NightAtticController.cs`** (신규)
- 망원경 → `Popup_NightTelescope` / 책장 → `Popup_ObservationJournal` / 침대 → Dream

**`NightTelescopePopupUI.cs`** (`Popup_NightTelescope`) (신규)
- "낚아올리자" → Fishing / "잠깐 기다려" → 닫기 / "…" → 엔딩 (Enlightenment >= 80 조건)

**`FishingTimerUI.cs` 수정**
- `IPointerClickHandler` 추가 → 클릭 시 `FishingPhaseController.SkipFishing()`

---

### Phase 4 — 꿈 파트 + VN 시스템

**`DreamEventData.cs`** (ScriptableObject 신규)
- `DreamNode[]`: Dialogue / Choice / Dice / End 타입
- 되감기 횟수, 성공 임계값, 분기 nodeId

**`DreamKeySelectionUI.cs`** (`Panel_DreamKeySelection`) (신규)
- CosmicTrace 레코드 선택 UI → `DreamEventRunner.StartEvent(record)`
- 레코드 없으면 DayAttic 복귀

**`DiceRollPopupUI.cs`** (`Popup_DiceRoll`) (신규)
- 주사위 굴리기 + 되감기(횟수 소비) + 자동 확정

**`ChoicePopupUI.cs`** (`Popup_Choice`) (신규)
- 최대 4선택지 동적 생성, Enlightenment 조건 비활성

**`DreamVNPanel.cs`** (`Panel_DreamVN`) (신규)
- 우측 프레임 전용 VN 패널, DialogueSystem 위임

**`DreamBaseController.cs`** (`Panel_Dream`) (신규)
- Dream Phase 진입 시 DreamVN + DreamKeySelection 활성화

**`DreamEventRunner.cs`** (신규)
- Dialogue/Choice/Dice/End 노드 FSM
- 완료 시 Enlightenment >= 80 → 엔딩 / 아니면 SaveScreen

---

### 화면 전환 흐름 요약

```
Title(1) → Prologue(2) → DayAttic(3/14)
DayAttic → DayCity(4/15) → 학회(5/16) / 대학(6/17) / 상점(7/18)
DayAttic → NightA(8/19) → Fishing(9/10/20) → NightB → Dream(11/12/13) → Save(22) → DayAttic
NightA → Dream 직접 가능
Dream → 엔딩 조건 충족 시 → Ending(25) → Title(1)
```

### Inspector 연결 필요 (Unity 에디터 작업)

- `GameFlowDirector`: `prologueData`, `endingData` SO 연결
- `DreamEventRunner`: `availableEvents` 배열 연결 또는 `Resources/Dream/Events/` 에 배치
- `RightFrameContentController`: Phase별 배경 Sprite, NPC 초상화 연결
- 각 UI 프리팹 Button/Image/Text 필드 Inspector 연결

---

## [2026-03-29] 우주 낚시 시스템 기초 구현

### 작업 배경

UIUX 레파토리 기획서와 우주 낚시 시스템 기획서를 바탕으로 게임의 핵심 루프를 구현했다.
기존 코드베이스는 Idle Game 프로토타입(정원 배치, 건물, 오프라인 생산) 기반이었으나,
해당 씬은 모두 주석 처리 상태였기 때문에 새 시스템은 기존 인프라(SingletonBase, UIManager, InputSystem)만 재사용하고 처음부터 설계했다.

### 구현 범위

#### 데이터 레이어

**`PlayerParameters.cs`**
- 4대 파라미터(명성 Fame / 이성 Sanity / 계몽 Enlightenment / 광기 Madness) + 자금(Funds) 관리
- 각 파라미터는 0~100 float, 변경 시 `OnParameterChanged(ParameterType, float)` 이벤트 발행
- `ParametersSaveData` 내부 클래스로 JsonUtility 직렬화 지원 (FileManager 연동은 TODO)

**`ObservationRecord.cs`**
- `RecordType` (CelestialBody / Phenomenon / CosmicTrace), `Rarity` (Common ~ Legendary) 열거형
- `ObservationRecord` 단일 레코드 클래스
- `ObservationJournal` 싱글톤: 전체 기록 + 미처분 기록(pendingRecords) 분리 관리
- `DisposalMethod` 4종 처분 로직: 발표(명성+자금) / 파기(이성) / 탐구(계몽) / 광기(광기)
- Rarity 배율: Common×1 / Uncommon×1.5 / Rare×2.5 / Legendary×5

**`TelescopeData.cs`**
- 7종 파트(Lens / Filter / Handle / OpticalAdjuster / FocusTracker / SignalAmplifier / RecordingDevice) 각 1~5레벨
- 파트별 희귀도 보너스 분리: Filter→CelestialBody, SignalAmplifier+FocusTracker→Phenomenon, OpticalAdjuster+RecordingDevice→CosmicTrace
- JsonUtility Dictionary 미지원 문제 → `int[]` 병렬 배열로 직렬화 변환

#### 페이즈 시스템

**`PhaseManager.cs`**
- `GamePhase` 6종: DayAttic / DayCity / NightA / Fishing / NightB / Dream
- 유효 전환 규칙 검증 포함 (예: NightB에서 Fishing으로 되돌아갈 수 없음)
- DayAttic↔DayCity 전환은 날짜 미증가, Dream→DayAttic 전환 시 `CurrentDay++`
- `OnPhaseChanged(oldPhase, newPhase)` 이벤트로 전체 씬 오브젝트가 구독

#### UI 레이어

**`UIList.cs` 업데이트**
- 기존 Idle Game UI 항목 유지 + Starry Night 항목 추가
- Panel: MainLayout / HUD_Parameters / Panel_DayAttic / Panel_DayCity / Panel_NightAttic / Panel_Fishing / Panel_Dream
- Popup: Popup_Inventory / Popup_ObservationJournal / Popup_TelescopeUpgrade / Popup_RecordDisposal / Popup_SpaceMap / Popup_FocusMinigame

**`HUD_Parameters.cs`**
- 4 파라미터 + 자금 표시 (TextMeshPro)
- 명성 좌상단 / 이성 우상단 / 계몽 좌하단 / 광기 우하단 배치
- `OnEnable`/`OnDisable`에서 이벤트 구독/해제 (누수 없음)

**`MainLayoutController.cs`**
- 좌우 2분할 레이아웃 영구 프레임 컨트롤러
- 좌측: 원형 마스크 프레임 (사이드뷰 필드)
- 우측: 사각형 프레임 (POV / SCG 대화)
- 대화창: 화자명 + 대화문 + 자동재생 버튼 + 로그 버튼
- 페이즈 전환 시 `OnPhaseContentRequested` 이벤트 발행 → 콘텐츠 교체 책임은 외부로 위임

#### 방 인터랙션 시스템

**`InteractableObject.cs`** (기반 클래스)
- Collider2D 기반 마우스오버 감지
- `MaterialPropertyBlock`으로 `_OutlineEnabled` / `_OutlineColor` / `_OutlineWidth` 조작
- `IsInteractable` 플래그로 활성/비활성 전환

**방 오브젝트 구체 클래스** (`RoomObjects/`)

| 클래스 | 낮 동작 | 밤A 동작 | 밤B 동작 |
|---|---|---|---|
| `TelescopeObject` | 업그레이드 팝업 | 낚시 페이즈 진입 | 비활성 |
| `BookshelfObject` | 천체 도감 팝업 | 인벤토리 팝업 | 인벤토리 팝업 |
| `BedObject` | 밤A로 이행 | 낮으로 건너뛰기 | 낮으로 건너뛰기 |
| `IlusanObject` | 비활성 | CosmicTrace 보유 시 꿈 진입 | 동일 |

#### 낚시(우주 관측) 시스템

**`Fishing/ObservationZone.cs`**
- 관측 구역 정의 클래스 + 기본 3구역 하드코딩 (Near Orbit / Deep Space / Void Sector)
- 구역별 `availableTypes[]`, `rarityWeights[]`, `requiredLensLevel` 설정

**`Fishing/SpaceMapController.cs`**
- 렌즈 레벨에 따라 접근 가능한 구역 버튼 활성화
- 구역 선택 시 `OnZoneSelected(ObservationZone)` 이벤트 발행

**`Fishing/FishingPhaseController.cs`**
- 300초 타이머 (`totalTime` 인스펙터 조정 가능)
- `OnTimerUpdated(float)` 이벤트로 UI 타이머 연동
- 타이머 종료 또는 `SkipFishing()` 호출 시 NightB로 전환

**`Fishing/SignalPoint.cs`**
- 우주 공간 이상 신호 포인트 오브젝트
- 클릭 시 `FocusMiniGameController.Instance.StartMinigame(this)` 호출
- 한 번 상호작용 후 비활성화

**`Fishing/FocusMiniGameController.cs`**
- 포인터 좌우 왕복 타이밍 클릭 미니게임
- GreenZone 너비 = `baseGreenZoneWidth + Handle레벨 × greenZoneWidthPerLevel`
- 성공 시 `ObservationRecord` 생성: RecordType은 구역 가중치 랜덤, Rarity는 망원경 보너스 적용
- `InputSystem.Singleton.OnInput_Shoot` 이벤트 재활용 (미니게임 활성 중에만 구독)

### 빌드 결과

```
경고 0개 / 오류 0개
```

### 충돌 해소 내역

- `ObservationRecord.cs`가 두 에이전트에 의해 중복 생성 (`Common/` + `Fishing/`)
  → `Fishing/ObservationRecord.cs` 삭제, `FocusMiniGameController`의 생성자 및 필드명 통일
- `TelescopePart` enum 명칭 불일치 → `TelescopePartType`으로 통일

### 남은 작업 (TODO)

- [ ] Unity 씬에서 프리팹 생성 및 인스펙터 연결
- [ ] `MainLayout` 캔버스 프리팹 제작 (좌측 원형 마스크 + 우측 사각형 프레임)
- [ ] `HUD_Parameters` 프리팹 제작 (TextMeshPro 레이블 배치)
- [ ] 아웃라인 셰이더 연동 (`_OutlineEnabled` 파라미터 지원)
- [ ] `FileManager` 연동: PlayerParameters / ObservationJournal / TelescopeData 저장/로드 구현
- [ ] 낚시 필드: SignalPoint 스폰 로직 (구역별 밀도 및 이동 패턴)
- [ ] 꿈 페이즈: 열쇠 시스템 + VN 다이얼로그 엔진
- [ ] 낮 페이즈: 천문학회 기록 처분 UI (`Popup_RecordDisposal`)
- [ ] 도시 필드: 대학(업그레이드 해금) / 상점가(꾸미기) 씬 구현
- [ ] BGM 컨트롤러 UI

---

## [2026-03-29] 낚시 페이즈 프로토타입 씬 구성

### 작업 배경

낚시 페이즈 GDD를 바탕으로 Unity 씬에 실제 플레이 가능한 프로토타입을 구성했다.
기존에 스크립트만 존재하던 상태에서 씬 배치, 이동 시스템, 카메라 연동까지 진행했다.

### 구현 범위

#### 씬 오브젝트 배치

- `Plane` (scale 5,1,5 — 50×50 유닛)을 프로토타입 필드로 사용
- `FieldObjects` 빈 오브젝트 하위에 필드 오브젝트 정리 (Y=0.1)

| 종류 | 개수 | 배치 위치 |
|---|---|---|
| FishingGround (어장) | 4개 | (-8,0.1,5) / (10,0.1,8) / (-5,0.1,15) 외 1 |
| Hazard (위험 요소) | 2개 | (5,0.1,10) / (-12,0.1,12) |
| RecordArchive (기록 보관소) | 2개 | (0,0.1,18) / (12,0.1,16) |

- `Vessel` 중앙 출발 위치 (0, 0.1, 0)
- 테스트용 `Capsule` 비활성화 (보존)

#### 관측선 이동 시스템 (`VesselController.cs`)

- 입력 레거시 `Input.GetKey` → `UnityEngine.InputSystem` (`Keyboard.current`) 교체
- WASD + 화살표 키 모두 지원

| 파라미터 | 기본값 |
|---|---|
| maxForwardSpeed | 10f |
| backwardSpeedRatio | 0.3 (최대 3f) |
| acceleration | 5f |
| deceleration | 8f |
| turnSpeed | 90°/초 |
| stationaryThreshold | 0.1f |

- `IsStationary` 프로퍼티: 현재 속도 ≤ stationaryThreshold 시 true (상호작용 조건)
- `OnValidate`로 Inspector 수정 시 후진 속도 즉시 갱신

#### VesselCamera — RenderTexture 방식으로 전환

기존 Viewport Rect 방식에서 RenderTexture 방식으로 전면 교체.
CinemachineCamera + Screen Space Overlay Canvas 환경에서 Viewport Rect는 Cinemachine Brain 충돌 위험이 있어 RenderTexture가 적합하다고 판단.

- `VesselCamera` → 런타임 생성 `RenderTexture` → `LeftFrame/VesselView (RawImage)` 표시
- `VesselCameraController.cs`: LeftFrame 크기 기준으로 Start()에서 RenderTexture 동적 생성
- OnDestroy()에서 `RenderTexture.Release()` + `Destroy()` 처리

#### Ship_Proto 스프라이트 6분할

`Assets/PROJECT-TSN/Arts/Asset_Proto_1/Ship_Proto.png` (96×64px) — 32×32 단위 6슬라이스

| 슬라이스 | 위치 (x, y) |
|---|---|
| Ship_DiagLeft | (0, 32) |
| Ship_Up | (32, 32) |
| Ship_DiagRight | (64, 32) |
| Ship_Left | (0, 0) |
| Ship_Down | (32, 0) |
| Ship_Right | (64, 0) |

- `.meta` 파일 수정으로 처리 (`spriteMode: 2` Multiple, sprites 배열 6개)

#### MCP 도구 버그 수정 (`SetComponentPropertyTool.cs`)

- `BindingFlags.NonPublic` 추가 → private SerializeField 필드 탐색 가능
- 상속 계층 순회 추가 → 부모 클래스 필드 탐색
- `typeof(UnityEngine.Object)` 분기 추가 → instanceId 기반 Object 참조 할당 가능

### 남은 작업 (TODO)

- [ ] 관측선 스프라이트 방향 전환 (`VesselSpriteController` — Ship_Proto 6분할 슬라이스 연결)
- [ ] 활동 경계선 시스템 (필드 범위 초과 경고 + 침몰 판정)
- [ ] 패럴랙스 레이어 구성 (배경 / 장식 / 오브젝트 레이어)
- [ ] 하단 HUD 연결 (조타륜 방향, 속도 바, 내구도 바)
- [ ] 존 시스템 (구역 이동 시 배경만 교체)
- [ ] ObservationZone 3개 → 9개 확장

---

## [2026-03-30] 낚시 페이즈 핵심 시스템 완성

### 작업 배경

낚시 페이즈 프로토타입 씬에 이어 GDD 사양에 따라 관측선 이동·내구도·오브젝트·HUD 등 미구현 시스템을 모두 완성했다.

### 구현 범위

#### 관측선 시스템

**`VesselSpriteController.cs`**
- 8방향 스프라이트 교체 컨트롤러
- `VesselController.FacingAngle` 기반 45° 구간 인덱스 계산 (0=N, 1=NE, … 7=NW)
- 방향 변경 시에만 스프라이트 교체 (불필요한 갱신 방지)

**`VesselDirectionPointer.cs`**
- 발밑 원형 방향 포인터 (바닥면 수평 눕힘 스프라이트)
- `LateUpdate`에서 `FacingAngle`에 맞춰 Y 회전

**`VesselHull.cs`**
- 관측선 내구도 시스템 (최대 100, 기본값 인스펙터 설정 가능)
- `TakeDamage(float)` / `Recover(float)` API
- `OnDurabilityChanged(float)` 이벤트 — UI 바인딩용
- 내구도 0 → `VesselController.ForceStop()` + `FishingPhaseController.EndFishing()` 자동 호출
- 침몰 중복 방지 플래그 (`_isSunk`)

#### 필드 오브젝트

**`Hazard.cs`**
- 위험 요소 필드 오브젝트 (`Collider2D` isTrigger 기반)
- `OnTriggerEnter2D` / `OnTriggerStay2D`에서 "Vessel" 태그 감지
- 충돌 쿨다운(`damageCooldown`) 적용 — 매 프레임 중복 피해 방지

**`FishingGround.cs`**
- 어장 오브젝트 (`InteractableObject` 확장)
- **정선 조건 적용**: `VesselController.IsStationary` 체크 후 미니게임 시작
- zone 미설정 시 `FishingPhaseController.CurrentZone`으로 자동 폴백
- `respawnTime` 후 재활성화 코루틴

**`RecordArchive.cs`**
- 기록 보관소 오브젝트 (`InteractableObject` 확장)
- **정선 조건 적용**: `VesselController.IsStationary` 체크
- 상호작용 시 내구도 회복 + `ObservationJournal.Save()` 자동 호출
- `cooldownTime` 후 재활성화 코루틴

#### UI

**`FishingHudUI.cs`**
- 낚시 페이즈 전용 하단 HUD (`UIList.Panel_FishingHUD`)
- 조타륜(helmImage): `FacingAngle` → UI Z축 반전 회전
- 내구도 바: `VesselHull.OnDurabilityChanged` 이벤트 구독, Show/Hide 시 자동 바인딩·해제
- 속도 바: `Update`에서 `VesselController.SpeedRatio` 폴링

### 빌드 결과

```
경고 0개 / 오류 0개
```

### 남은 작업 (TODO)

- [x] `SignalPoint` — 정선 조건 추가 → 2026-03-30 완료
- [x] `FocusMiniGameController` — 세로 스크롤 그래프 방식으로 교체 → 2026-03-30 완료
- [x] `ObservationZone` — ScriptableObject 마이그레이션 + 9구역 확장 → 2026-03-30 완료
- [x] 패럴랙스 레이어 필드 → 2026-03-30 완료
- [x] 활동 경계선 시스템 → 2026-03-30 완료
- [x] 우측 프레임 타이머 → 2026-03-30 완료

---

## [2026-03-30] 낚시 페이즈 잔여 시스템 구현 및 씬 MCP 연결

### 작업 배경

GDD 미구현 항목 전체를 스크립트 작성 + MCP ToryAgent를 통한 씬 Inspector 연결까지 완료했다.

### 구현 범위

#### 스크립트

**`SignalPoint.cs` 수정**
- `OnMouseDown`에 `VesselController.IsStationary` 정선 조건 추가
- 정선 상태가 아니면 클릭 무시

**`ObservationZone.cs` → ScriptableObject 마이그레이션**
- `[Serializable] class` → `ScriptableObject` 전환
- `[CreateAssetMenu(menuName = "TST/Fishing/ObservationZone")]` 추가
- `CreateDefaultZones()` 팩토리 메서드 제거
- `SpaceMapController.cs` — `zones` 필드를 Inspector 연결 방식(`List<ObservationZone>`)으로 변경

**`CreateObservationZoneAssets.cs` (Editor)**
- `Tools > TST > Create Observation Zone Assets` 메뉴로 9개 SO 에셋 일괄 생성
- `Assets/PROJECT-TSN/ScriptableObjects/ObservationZones/` 저장
- 구역 구성: zone_1~3 (LensLv1), zone_4~6 (LensLv2), zone_7~9 (LensLv3)

**`FishingTimerUI.cs` (신규)**
- 우측 프레임 시계형 원호 타이머 (`UIList.Panel_FishingTimer`)
- `arcImage`: `Image Type=Filled / Radial360 / FillOrigin=Top / Clockwise`
- `needleImage`: fillAmount 기반 Z축 회전
- `FishingPhaseController.OnTimerUpdated` 이벤트 구독

**`FishingPhaseController.cs` 수정**
- `TotalTime` 읽기 전용 프로퍼티 추가 (FishingTimerUI 연동용)

**`FocusMiniGameController.cs` 교체**
- 좌우 포인터 단발 클릭 방식 → 세로 스크롤 그래프 + 원호형 타이밍 바 복합 방식
- `ScrollGraphValue` (0~1): 매 프레임 스크롤 속도만큼 감소
- 클릭 성공 → `+jumpUpAmount`, 실패 → `-jumpDownAmount`
- `ScrollGraphValue >= 1` → 최종 성공, `<= 0` → 최종 실패
- 성공/실패 후 GreenZone 위치 랜덤 재배치
- Rarity별 scrollSpeedMult / zoneWidthMult 배율 적용 (난이도 조절)

**`ParallaxLayerController.cs` (신규)**
- 7개 레이어 패럴랙스 시스템 (Background 0.05 ~ Vessel 1.0)
- `LateUpdate`에서 VesselController XZ 이동 델타 × parallaxFactor 적용
- `SnapToCurrent()` / `ResetLayers()` API

**`ActivityBoundary.cs` (신규)**
- 원형 경계(`boundaryRadius`) 초과 감지
- 초과 지속 시 `OnBoundaryVignetteChanged(0~1)` / `OnBoundaryWarningChanged(bool)` 이벤트 발행
- `returnTimeLimit` 초 경과 → `VesselHull.TakeDamage(sinkDamage)` 호출 → 침몰
- Scene 뷰 Gizmo 원 표시

#### MCP ToryAgent 씬 작업

| 작업 | 결과 |
|---|---|
| ObservationZone SO 9개 에셋 생성 | `ScriptableObjects/ObservationZones/zone_1~9.asset` |
| SpaceMapController 생성 + zones 9개 연결 | 완료 |
| FishingGround 3개 zone 필드 연결 | zone_1 연결 |
| ParallaxController GameObject 생성 | 레이어 7개 Transform + factor 연결 |
| ActivityBoundary GameObject 생성 | radius=40, returnTime=10, damage=100 |
| Panel_FishingTimer + FishingTimerUI 생성 | arcImage / needleImage 연결 |
| 씬 저장 | 완료 |

### 남은 Inspector 작업

- `ArcImage` / `NeedleImage`에 실제 Sprite 연결 필요
- `SpaceMapController.zoneButtons` — UI 버튼 생성 후 연결 필요
- `ActivityBoundary.OnBoundaryVignetteChanged` → Post Processing Vignette 연결
- `ActivityBoundary.OnBoundaryWarningChanged` → 경고 UI 패널 연결
