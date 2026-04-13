using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TST
{
    /// <summary>
    /// 낚시 관련 페이즈 전환 시 암전 시퀀스를 담당합니다.
    ///
    /// BeginTransition()      : NightA → Space   (암전 + Space 씬 오브젝트 교체 + 페이즈 전환)
    /// BeginFishingTransition(): Space → Fishing (암전 + Fishing 씬 오브젝트 교체 + 페이즈 전환)
    ///
    /// NightA→Space 시퀀스:
    ///   1. Post Exposure 0 → fadeTargetEV  (암전)
    ///   2. Space 씬 오브젝트 교체 (spaceBG 켜기 / transitionCamera2D 유지 또는 교체)
    ///   3. PhaseManager.TransitionTo(Space) ← NightAtticController가 atticRoot를 자동 비활성화
    ///   4. Post Exposure fadeTargetEV → 0   (암전 해제, Space 화면 표시)
    ///
    /// Space→Fishing 시퀀스:
    ///   1. Post Exposure 0 → fadeTargetEV  (암전)
    ///   2. Fishing 씬 오브젝트 교체 (spaceBG 끄기 / GridGround·PlayerCamera·playerObject 켜기)
    ///   3. PhaseManager.TransitionTo(Fishing)
    ///   4. Post Exposure fadeTargetEV → 0   (암전 해제)
    ///
    /// Fishing→NightB 시퀀스 (OnFishingEnded 이벤트로 자동 트리거):
    ///   1. Post Exposure 0 → fadeTargetEV  (암전)
    ///   2. Fishing 씬 오브젝트 정리 (gridGround·playerCamera·playerObject 끄기 / transitionCamera2D 켜기)
    ///   3. Post Exposure fadeTargetEV → 0   (암전 해제, NightAtticController가 atticRoot 자동 활성화)
    ///
    /// Inspector 와이어링:
    ///   globalVolume       — Global Volume (ColorAdjustments override 포함)
    ///   spaceBG            — Space(BG) GameObject (필드 선택 화면 배경)
    ///   gridGround         — 낚시 필드 그리드 루트 오브젝트 (FishingObjs)
    ///   playerCamera       — 낚시 씬 PlayerCamera GameObject
    ///   transitionCamera2D — NightAttic용 2D TransitionCamera GameObject
    ///   playerObject       — 캐릭터 루트 GameObject
    ///   fadeDuration       — 암전 / 해제 각각의 소요 시간(초)
    ///   fadeTargetEV       — 암전 도달 EV 값 (기본 -10, 낮을수록 더 어두움)
    /// </summary>
    public class FishingTransitionController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────
        [Header("Post Processing")]
        [SerializeField] private Volume globalVolume;
        [SerializeField] private float  fadeDuration  = 0.5f;
        [SerializeField] private float  fadeTargetEV  = -10f;

        [Header("Space Scene Objects")]
        [SerializeField] private GameObject spaceBG;

        [Header("Fishing Scene Objects")]
        [SerializeField] private GameObject gridGround;
        [SerializeField] private GameObject playerCamera;
        [SerializeField] private GameObject transitionCamera2D;
        [SerializeField] private GameObject playerObject;

        [Header("Dependencies")]
        [SerializeField] private FishingPhaseController fishingPhaseController;

        // ── 런타임 ───────────────────────────────────────────────────
        private ColorAdjustments _colorAdjustments;
        private ObservationZone  pendingZone;

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            if (globalVolume != null)
                globalVolume.profile.TryGet(out _colorAdjustments);

            if (_colorAdjustments == null)
                Debug.LogWarning("[FishingTransitionController] ColorAdjustments override를 찾지 못했습니다. " +
                                 "Global Volume 프로파일에 ColorAdjustments를 추가하세요.");

            if (fishingPhaseController != null)
                fishingPhaseController.OnFishingEnded += OnFishingEnded;
        }

        private void OnDestroy()
        {
            if (fishingPhaseController != null)
                fishingPhaseController.OnFishingEnded -= OnFishingEnded;
        }

        private void OnFishingEnded()
        {
            StartCoroutine(FishingExitRoutine());
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// NightA → Space 암전 전환을 시작합니다.
        /// NightTelescopePopupUI.OnFishingClicked에서 호출합니다.
        /// </summary>
        public void BeginTransition()
        {
            StartCoroutine(SpaceTransitionRoutine());
        }

        /// <summary>
        /// Space → Fishing 암전 전환을 시작합니다.
        /// SpaceMapController.SelectZone에서 호출합니다.
        /// </summary>
        public void BeginFishingTransition(ObservationZone zone)
        {
            pendingZone = zone;
            StartCoroutine(FishingTransitionRoutine());
        }

        // ── 시퀀스 ───────────────────────────────────────────────────

        private IEnumerator SpaceTransitionRoutine()
        {
            // 1. 암전
            yield return StartCoroutine(FadeExposure(0f, fadeTargetEV, fadeDuration));

            // 2. Space 씬 오브젝트 교체 (화면이 검을 때)
            SwapToSpaceObjects();

            // 3. 페이즈 전환 — NightAtticController.HandlePhaseChanged가 atticRoot를 자동 끔
            PhaseManager.Singleton.TransitionTo(GamePhase.Space);

            // 4. 암전 해제 (Space 필드 선택 화면 표시)
            yield return StartCoroutine(FadeExposure(fadeTargetEV, 0f, fadeDuration));
        }

        private IEnumerator FishingTransitionRoutine()
        {
            // 1. 암전
            yield return StartCoroutine(FadeExposure(0f, fadeTargetEV, fadeDuration));

            // 2. Fishing 씬 오브젝트 교체 (화면이 검을 때)
            SwapToFishingObjects();

            // 3. 페이즈 전환
            PhaseManager.Singleton.TransitionTo(GamePhase.Fishing);

            // 4. 암전 해제
            yield return StartCoroutine(FadeExposure(fadeTargetEV, 0f, fadeDuration));

            // 5. 페이드인 완료 후 낚시 세션 시작 (HUD 포함)
            fishingPhaseController?.StartFishing(pendingZone);
            pendingZone = null;
        }

        private IEnumerator FishingExitRoutine()
        {
            // 1. 암전
            yield return StartCoroutine(FadeExposure(0f, fadeTargetEV, fadeDuration));

            // 2. Fishing 씬 오브젝트 정리 (화면이 검을 때)
            SwapFromFishingObjects();

            // 3. 페이즈 전환 — NightAtticController가 atticRoot를 자동 활성화
            PhaseManager.Singleton.TransitionTo(GamePhase.NightB);

            // 4. 암전 해제
            yield return StartCoroutine(FadeExposure(fadeTargetEV, 0f, fadeDuration));
        }

        private void SwapFromFishingObjects()
        {
            if (gridGround         != null) gridGround.SetActive(false);
            if (playerCamera       != null) playerCamera.SetActive(false);
            if (transitionCamera2D != null) transitionCamera2D.SetActive(true);
            if (playerObject       != null) playerObject.SetActive(false);
        }

        private void SwapToSpaceObjects()
        {
            if (spaceBG            != null) spaceBG.SetActive(true);
            if (playerObject       != null) playerObject.SetActive(false);
        }

        private void SwapToFishingObjects()
        {
            if (spaceBG            != null) spaceBG.SetActive(false);
            if (gridGround         != null) gridGround.SetActive(true);
            if (playerCamera       != null) playerCamera.SetActive(true);
            if (transitionCamera2D != null) transitionCamera2D.SetActive(false);
            if (playerObject       != null) playerObject.SetActive(true);
        }

        // ── 페이드 헬퍼 ──────────────────────────────────────────────

        private IEnumerator FadeExposure(float from, float to, float duration)
        {
            if (_colorAdjustments == null)
            {
                yield break;
            }

            _colorAdjustments.postExposure.Override(from);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _colorAdjustments.postExposure.Override(Mathf.Lerp(from, to, t));
                yield return null;
            }

            _colorAdjustments.postExposure.Override(to);
        }
    }
}
