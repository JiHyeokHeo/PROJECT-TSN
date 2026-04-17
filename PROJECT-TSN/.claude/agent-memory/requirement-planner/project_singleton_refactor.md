---
name: 싱글톤 리팩터링 분석 및 계획
description: 싱글톤 남용 문제에 대한 클래스별 분류, 대체 패턴, 우선순위 계획
type: project
---

싱글톤 리팩터링 분석을 수행함 (2026-04-02).

**Why:** 낚시 페이즈 FishingHudUI 관리 논의 중 싱글톤이 과하다는 결론에 도달. 씬 종속적인 Controller들이 DontDestroyOnLoad 상속을 받고 있는 문제.

**분류 결과:**

유지 그룹 (앱 수명주기 일치):
- PhaseManager, SaveSystem, SoundManager, InputSystem, PlayerParameters

대체 필요 그룹 (씬 종속, Inspector 직렬화 가능):
- FishingPhaseController, VesselController, VesselHull, SpaceMapController, FocusMiniGameController

이미 올바른 패턴 (수동 Singleton):
- VesselCameraController (씬에서 할당, OnDestroy에서 null 처리)
- GameManager (수동 Instance 패턴)

비활성화된 클래스:
- GameDataModel, UserDataModel, ResourceManager (전부 주석 처리됨 — 현재 미사용)

**How to apply:** 새 씬 종속 컨트롤러 작성 시 SingletonBase 상속 금지. 대신 씬 컴포넌트 + Inspector 참조 패턴 사용.
