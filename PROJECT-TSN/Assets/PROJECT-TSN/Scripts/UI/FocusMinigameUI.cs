using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 초점 맞추기 미니게임 팝업 UI.
    /// FocusMiniGameController 의 상태(PointerPos, GreenCenter, GreenWidth, State)를
    /// 매 프레임 읽어 arc 위에서 포인터·성공 구간을 표시합니다.
    ///
    /// Inspector 와이어링:
    ///   arcBarImage      — ArcBar (Image, Type = Filled, Fill Method = Radial 360)
    ///   greenZoneImage   — GreenZone (Image, green)
    ///   pointerImage     — Pointer (Image, white circle)
    ///   resultOverlay    — 성공·실패 페이드용 반투명 오버레이 Image (optional)
    ///
    /// Arc 정의:
    ///   ARC_START_DEG ~ ARC_END_DEG (시계 방향, UI 각도 기준)
    ///   0.0 → ARC_START_DEG, 1.0 → ARC_END_DEG
    /// </summary>
    public class FocusMinigameUI : UIBase
    {
        // ── Arc 설정 ─────────────────────────────────────────────────────
        /// <summary>arc 의 시작 각도 (도, 12시 = 0, 시계방향 양수).</summary>
        private const float ARC_START_DEG = -140f;

        /// <summary>arc 의 끝 각도.</summary>
        private const float ARC_END_DEG = 140f;

        /// <summary>arc 의 전체 각도 범위.</summary>
        private const float ARC_RANGE_DEG = ARC_END_DEG - ARC_START_DEG; // 280

        // ── 성공·실패 연출 ────────────────────────────────────────────
        private const float RESULT_FADE_IN_DURATION  = 0.15f;
        private const float RESULT_HOLD_DURATION     = 0.5f;
        private const float RESULT_FADE_OUT_DURATION = 0.3f;

        private static readonly Color COLOR_SUCCESS = new Color(0.2f, 0.9f, 0.3f, 0.8f);
        private static readonly Color COLOR_FAIL    = new Color(0.9f, 0.2f, 0.2f, 0.8f);

        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Arc Elements")]
        [Tooltip("GreenZone 이미지 (arc 위에 배치된 Image 컴포넌트)")]
        [SerializeField] private RectTransform greenZoneRect;

        [Tooltip("포인터 이미지 RectTransform")]
        [SerializeField] private RectTransform pointerRect;

        [Tooltip("Arc 중심으로 사용할 RectTransform (ArcBar 자체 또는 별도 pivot)")]
        [SerializeField] private RectTransform arcPivot;

        [Tooltip("Arc 반지름 (픽셀, arcPivot 기준)")]
        [SerializeField] private float arcRadius = 180f;

        [Header("Result Overlay (optional)")]
        [Tooltip("성공/실패 시 페이드 연출에 사용할 반투명 Image. null 이면 연출 생략.")]
        [SerializeField] private Image resultOverlay;

        // ── 런타임 ───────────────────────────────────────────────────
        private FocusMiniGameController.MiniGameState _lastState
            = FocusMiniGameController.MiniGameState.Idle;

        private bool  _isPlayingResult;
        private float _resultTimer;
        private Color _resultTargetColor;

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            base.Show();
            _isPlayingResult = false;
            if (resultOverlay != null)
                resultOverlay.color = Color.clear;
        }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Update()
        {
            if (FocusMiniGameController.Singleton == null) return;

            FocusMiniGameController ctrl = FocusMiniGameController.Singleton;

            // 포인터 위치 갱신 (매 프레임)
            UpdatePointer(ctrl.PointerPos);

            // 성공 구간 갱신
            UpdateGreenZone(ctrl.GreenCenter, ctrl.GreenWidth);

            // 상태 전환 감지 → 연출 트리거
            if (ctrl.State != _lastState)
            {
                OnStateChanged(ctrl.State);
                _lastState = ctrl.State;
            }

            // 결과 오버레이 페이드 진행
            if (_isPlayingResult)
                TickResultOverlay();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        /// <summary>정규화 위치(0~1) → arc 각도 → pointerRect 위치·회전 적용.</summary>
        private void UpdatePointer(float normalizedPos)
        {
            if (pointerRect == null || arcPivot == null) return;

            float angle = NormalizedToArcAngle(normalizedPos);
            pointerRect.anchoredPosition = AngleToArcPosition(angle);
        }

        /// <summary>GreenZone의 중심과 너비를 arc 위 위치·각도로 변환해 적용.</summary>
        private void UpdateGreenZone(float center, float width)
        {
            if (greenZoneRect == null || arcPivot == null) return;

            // 중심 위치
            float centerAngle = NormalizedToArcAngle(center);
            greenZoneRect.anchoredPosition = AngleToArcPosition(centerAngle);

            // 너비 → arc 호의 픽셀 길이를 sizeDelta.x 로 반영
            float arcLengthPx = width * ARC_RANGE_DEG * Mathf.Deg2Rad * arcRadius;
            greenZoneRect.sizeDelta = new Vector2(Mathf.Max(4f, arcLengthPx), greenZoneRect.sizeDelta.y);

            // 중심 각도에 맞게 회전 (호를 따라 눕힘)
            greenZoneRect.localRotation = Quaternion.Euler(0f, 0f, -centerAngle);
        }

        /// <summary>정규화 0~1 → arc 각도(도, 시계 방향 UI 기준)</summary>
        private static float NormalizedToArcAngle(float t)
        {
            return ARC_START_DEG + t * ARC_RANGE_DEG;
        }

        /// <summary>arc 각도 → arcPivot 기준 anchoredPosition</summary>
        private Vector2 AngleToArcPosition(float angleDeg)
        {
            // Unity UI: 0° = 위쪽(+Y), 시계방향 양수
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(
                Mathf.Sin(rad)  * arcRadius,
                Mathf.Cos(rad)  * arcRadius
            );
        }

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

                case FocusMiniGameController.MiniGameState.Idle:
                    // FocusMiniGameController.EndMinigame() 이 UIManager.Hide 를 호출하므로
                    // 이 UI 는 곧 비활성화됩니다. 별도 처리 불필요.
                    break;
            }
        }

        private void BeginResultOverlay(Color targetColor)
        {
            if (resultOverlay == null) return;
            _resultTargetColor = targetColor;
            _resultTimer       = 0f;
            _isPlayingResult   = true;
            resultOverlay.color = Color.clear;
        }

        private void TickResultOverlay()
        {
            _resultTimer += Time.deltaTime;

            float total = RESULT_FADE_IN_DURATION + RESULT_HOLD_DURATION + RESULT_FADE_OUT_DURATION;

            if (_resultTimer <= RESULT_FADE_IN_DURATION)
            {
                float t = _resultTimer / RESULT_FADE_IN_DURATION;
                resultOverlay.color = Color.Lerp(Color.clear, _resultTargetColor, t);
            }
            else if (_resultTimer <= RESULT_FADE_IN_DURATION + RESULT_HOLD_DURATION)
            {
                resultOverlay.color = _resultTargetColor;
            }
            else if (_resultTimer <= total)
            {
                float t = (_resultTimer - RESULT_FADE_IN_DURATION - RESULT_HOLD_DURATION)
                          / RESULT_FADE_OUT_DURATION;
                resultOverlay.color = Color.Lerp(_resultTargetColor, Color.clear, t);
            }
            else
            {
                resultOverlay.color = Color.clear;
                _isPlayingResult = false;
            }
        }
    }
}
