# DEVLOG — The Starry Night

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
