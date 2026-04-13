using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 낚시 페이즈 우측 프레임 타이머 UI.
    ///
    /// 시계형 원호 그래프로 남은 시간을 표시합니다.
    ///   - arcImage    : Image Type = Filled, Fill Method = Radial 360,
    ///                   Fill Origin = Top(12시), Clockwise = true
    ///                   fillAmount = 1 - (RemainingTime / totalTime) 로 채워짐
    ///   - needleImage : fillAmount 비율에 따라 Z 축 회전하는 단침 이미지
    ///
    /// 타이머가 0에 도달하면 FishingPhaseController.EndFishing() 을 호출합니다.
    /// (FishingPhaseController.Update() 에도 동일 처리가 있으나 이 UI 는 단침이
    ///  한 바퀴를 완성하는 시각적 완료 시점을 명확히 표현하기 위해 별도로 감지합니다.)
    ///
    /// UIList: Panel_FishingTimer (Panel 범위)
    ///
    /// Inspector 와이어링:
    ///   arcImage    — Image, Filled / Radial 360 / Fill Origin Top / Clockwise
    ///   needleImage — Image (단침), pivot 은 시계 중심에 맞출 것
    /// </summary>
    public class FishingTimerUI : UIBase, IPointerClickHandler
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private FishingPhaseController fishingPhaseController;

        [Header("Arc")]
        [Tooltip("원호 이미지. Image Type = Filled, Fill Method = Radial 360, Fill Origin = Top, Clockwise = true.")]
        [SerializeField] private Image arcImage;

        [Header("Needle")]
        [Tooltip("단침 이미지. fillAmount 비율에 따라 Z 축 회전합니다. pivot 을 시계 중심에 맞춰야 합니다.")]
        [SerializeField] private Image needleImage;

        // ── 상수 ─────────────────────────────────────────────────────
        private const float FullRotationDeg = 360f;

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            base.Show();
            BindTimerEvent();
            SyncTimer();
        }

        public override void Hide()
        {
            UnbindTimerEvent();
            base.Hide();
        }

        // ── 이벤트 바인딩 ─────────────────────────────────────────────

        private void BindTimerEvent()
        {
            if (fishingPhaseController == null) return;
            fishingPhaseController.OnTimerUpdated += HandleTimerUpdated;
        }

        private void UnbindTimerEvent()
        {
            if (fishingPhaseController == null) return;
            fishingPhaseController.OnTimerUpdated -= HandleTimerUpdated;
        }

        // ── 핸들러 ───────────────────────────────────────────────────

        private void HandleTimerUpdated(float remainingTime)
        {
            ApplyTimer(remainingTime);
        }

        // ── IPointerClickHandler ─────────────────────────────────────

        /// <summary>
        /// 타이머 UI 클릭 시 낚시를 즉시 종료합니다 (일루산 타이머 인터랙션).
        /// Panel_FishingTimer 프리팹 루트 또는 arcImage 위에 GraphicRaycaster가
        /// 동작하는 Canvas가 있어야 클릭 이벤트가 전달됩니다.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (fishingPhaseController == null) return;
            if (!fishingPhaseController.IsActive) return;
            fishingPhaseController.SkipFishing();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        /// <summary>Show() 시점에 현재 타이머 상태로 즉시 동기화.</summary>
        private void SyncTimer()
        {
            if (fishingPhaseController == null) return;
            ApplyTimer(fishingPhaseController.RemainingTime);
        }

        /// <summary>
        /// 잔여 시간을 받아 원호와 단침을 갱신합니다.
        ///
        /// fillAmount = 1 - (remaining / total)
        ///   → 시간이 지날수록 호(arc)가 12시 방향에서 시계방향으로 채워집니다.
        ///
        /// 단침 각도 = fillAmount * 360° (Z 축 음수 방향, 즉 시계방향 회전)
        /// </summary>
        private void ApplyTimer(float remainingTime)
        {
            if (fishingPhaseController == null) return;

            float totalTime = fishingPhaseController.TotalTime;
            if (totalTime <= 0f) return;

            float fillAmount = 1f - Mathf.Clamp01(remainingTime / totalTime);

            if (arcImage != null)
                arcImage.fillAmount = fillAmount;

            if (needleImage != null)
            {
                // Clockwise 회전 = Z 축 음수 방향
                float angleDeg = -fillAmount * FullRotationDeg;
                needleImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
            }
        }
    }
}
