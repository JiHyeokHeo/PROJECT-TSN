using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 초점 맞추기 미니게임 팝업 UI.
    ///
    /// [UI 구성]
    ///   1. 아이콘 : 낚아올릴 천체 실루엣 (Image)
    ///   2. 세로 스크롤 바 (Scroll Bar 1):
    ///        - scrollBarBackground : 바 배경 RectTransform (기준 높이)
    ///        - scrollKnobRect      : 노란 원형 손잡이 (ScrollGraphValue → 상하 위치)
    ///        - successZoneRect     : 상단 녹색 성공 영역 (고정, SuccessThreshold 기준)
    ///        - failZoneRect        : 하단 붉은 실패 영역 (고정)
    ///   3. 원호형 바 (Scroll Bar 2):
    ///        - greenZoneRect       : 초록색 성공 구간
    ///        - pointerRect         : 파란 원형 포인터 (좌우 왕복)
    ///   4. 결과 오버레이 (optional): 성공·실패 페이드 연출
    ///
    /// Inspector 와이어링:
    ///   arcPivot       — ArcBar 중심 RectTransform
    ///   arcRadius      — arc 반지름 (픽셀)
    ///   scrollBarBackground — 세로 바 RectTransform (높이 기준으로 knob 위치 계산)
    /// </summary>
    public class FocusMinigameUI : UIBase
    {
        // ── Arc 설정 — 포인터/그린존 모두 Z localRotation으로 제어 ─────────

        // ── 결과 연출 ────────────────────────────────────────────────
        private const float RESULT_FADE_IN_DURATION  = 0.15f;
        private const float RESULT_HOLD_DURATION     = 0.5f;
        private const float RESULT_FADE_OUT_DURATION = 0.3f;

        private static readonly Color COLOR_SUCCESS = new Color(0.2f, 0.9f, 0.3f, 0.8f);
        private static readonly Color COLOR_FAIL    = new Color(0.9f, 0.2f, 0.2f, 0.8f);

        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private FocusMiniGameController focusMiniGameController;

        [Header("Icon")]
        [Tooltip("낚아올릴 천체 실루엣을 표시하는 Image 컴포넌트")]
        [SerializeField] private Image celestialIconImage;

        [Header("Scroll Bar 1 — Vertical")]
        [Tooltip("세로 바 배경 RectTransform (높이를 노브 이동 범위 계산에 사용)")]
        [SerializeField] private RectTransform scrollBarBackground;

        [Tooltip("노란 원형 손잡이 RectTransform. ScrollGraphValue(0=하단, 1=상단)로 이동")]
        [SerializeField] private RectTransform scrollKnobRect;

        [Tooltip("상단 녹색 성공 영역 RectTransform (SuccessThreshold 위치에 고정 배치)")]
        [SerializeField] private RectTransform successZoneRect;

        [Tooltip("하단 붉은 실패 영역 RectTransform (바 최하단에 고정 배치)")]
        [SerializeField] private RectTransform failZoneRect;

        [Header("Scroll Bar 2 — Arc")]
        [Tooltip("GreenZone 이미지 RectTransform (arc 위에 배치)")]
        [SerializeField] private RectTransform greenZoneRect;

        [Tooltip("포인터 이미지 RectTransform")]
        [SerializeField] private RectTransform pointerRect;

        [Tooltip("Arc 중심 RectTransform")]
        [SerializeField] private RectTransform arcPivot;

        [Tooltip("Arc 반지름 (픽셀)")]
        [SerializeField] private float arcRadius = 180f;

        [Header("Result Overlay (optional)")]
        [Tooltip("성공/실패 페이드 연출용 반투명 Image. null이면 생략.")]
        [SerializeField] private Image resultOverlay;

        // ── 런타임 ───────────────────────────────────────────────────
        private FocusMiniGameController.MiniGameState lastState
            = FocusMiniGameController.MiniGameState.Idle;

        private bool  isPlayingResult;
        private float resultTimer;
        private Color resultTargetColor;

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        private void Awake()
        {
            if (focusMiniGameController == null)
                focusMiniGameController = FocusMiniGameController.Singleton;
        }

        public override void Show()
        {
            base.Show();
            isPlayingResult = false;
            if (resultOverlay != null)
                resultOverlay.color = Color.clear;

            // 아이콘 적용
            if (celestialIconImage != null && focusMiniGameController != null)
            {
                Sprite icon = focusMiniGameController.CelestialIcon;
                celestialIconImage.sprite  = icon;
                celestialIconImage.enabled = icon != null;
            }

            // 세로 바 성공/실패 영역 위치 고정 배치
            PlaceFixedZones();
        }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Update()
        {
            if (focusMiniGameController == null) return;

            FocusMiniGameController ctrl = focusMiniGameController;

            // 세로 스크롤 노브 위치 갱신
            UpdateScrollKnob(ctrl.ScrollGraphValue);

            // 포인터·성공 구간 갱신 (원호형 바)
            UpdatePointer(ctrl.PointerPos);
            UpdateGreenZone(ctrl.GreenCenter, ctrl.GreenWidth);

            // 상태 전환 감지 → 연출 트리거
            if (ctrl.State != lastState)
            {
                OnStateChanged(ctrl.State);
                lastState = ctrl.State;
            }

            if (isPlayingResult)
                TickResultOverlay();
        }

        // ── 세로 스크롤 바 ────────────────────────────────────────────

        /// <summary>ScrollGraphValue(0~1) → 세로 바 내 노브 anchoredPosition.y 갱신.</summary>
        private void UpdateScrollKnob(float value)
        {
            if (scrollKnobRect == null || scrollBarBackground == null) return;

            float barHeight  = scrollBarBackground.rect.height;
            float halfHeight = barHeight * 0.5f;
            float knobHalf   = scrollKnobRect.rect.height * 0.5f;

            float minY = -halfHeight + knobHalf;
            float maxY =  halfHeight - knobHalf;

            float y = Mathf.Lerp(minY, maxY, value);
            scrollKnobRect.anchoredPosition = new Vector2(scrollKnobRect.anchoredPosition.x, y);
        }

        /// <summary>성공·실패 고정 영역을 바 높이에 맞춰 한 번 배치합니다.</summary>
        private void PlaceFixedZones()
        {
            if (scrollBarBackground == null) return;

            float barHeight  = scrollBarBackground.rect.height;
            float halfHeight = barHeight * 0.5f;

            // 성공 영역: SuccessThreshold 이상 (상단)
            if (successZoneRect != null && focusMiniGameController != null)
            {
                float threshold = focusMiniGameController.SuccessThreshold;
                float topY      = halfHeight;
                float bottomY   = Mathf.Lerp(-halfHeight, halfHeight, threshold);
                float height    = topY - bottomY;
                float centerY   = (topY + bottomY) * 0.5f;

                successZoneRect.anchoredPosition = new Vector2(successZoneRect.anchoredPosition.x, centerY);
                successZoneRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(4f, height));
            }

            // 실패 영역: 하단 고정 (0~10% 높이)
            if (failZoneRect != null)
            {
                float failHeight = barHeight * 0.08f;
                float centerY    = -halfHeight + failHeight * 0.5f;

                failZoneRect.anchoredPosition = new Vector2(failZoneRect.anchoredPosition.x, centerY);
                failZoneRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(4f, failHeight));
            }
        }

        // ── 원호형 바 ────────────────────────────────────────────────

        private void UpdatePointer(float angleDeg)
        {
            if (pointerRect == null) return;
            pointerRect.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        private void UpdateGreenZone(float centerDeg, float widthDeg)
        {
            if (greenZoneRect == null) return;

            greenZoneRect.localRotation = Quaternion.Euler(0f, 0f, centerDeg);

            float arcLengthPx = widthDeg * Mathf.Deg2Rad * arcRadius;
            greenZoneRect.sizeDelta = new Vector2(Mathf.Max(4f, arcLengthPx), greenZoneRect.sizeDelta.y);
        }

        // ── 상태 전환 연출 ────────────────────────────────────────────

        private void OnStateChanged(FocusMiniGameController.MiniGameState newState)
        {
            switch (newState)
            {
                case FocusMiniGameController.MiniGameState.Success:
                    BeginResultOverlay(COLOR_SUCCESS);
                    break;
                case FocusMiniGameController.MiniGameState.Fail:
                    BeginResultOverlay(COLOR_FAIL);
                    break;
            }
        }

        private void BeginResultOverlay(Color targetColor)
        {
            if (resultOverlay == null) return;
            resultTargetColor  = targetColor;
            resultTimer        = 0f;
            isPlayingResult    = true;
            resultOverlay.color = Color.clear;
        }

        private void TickResultOverlay()
        {
            resultTimer += Time.deltaTime;

            float total = RESULT_FADE_IN_DURATION + RESULT_HOLD_DURATION + RESULT_FADE_OUT_DURATION;

            if (resultTimer <= RESULT_FADE_IN_DURATION)
            {
                float t = resultTimer / RESULT_FADE_IN_DURATION;
                resultOverlay.color = Color.Lerp(Color.clear, resultTargetColor, t);
            }
            else if (resultTimer <= RESULT_FADE_IN_DURATION + RESULT_HOLD_DURATION)
            {
                resultOverlay.color = resultTargetColor;
            }
            else if (resultTimer <= total)
            {
                float t = (resultTimer - RESULT_FADE_IN_DURATION - RESULT_HOLD_DURATION)
                          / RESULT_FADE_OUT_DURATION;
                resultOverlay.color = Color.Lerp(resultTargetColor, Color.clear, t);
            }
            else
            {
                resultOverlay.color = Color.clear;
                isPlayingResult    = false;
            }
        }
    }
}
