using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 전용 하단 HUD.
    ///
    /// 표시 항목:
    ///   - 조타륜(helmImage): VesselController.FacingAngle 에 따라 Z 축 회전
    ///   - 내구도 바(durabilityBar): VesselHull.CurrentDurability / MaxDurability
    ///   - 속도 바(speedBar): VesselController.SpeedRatio (0~1)
    ///
    /// VesselHull 은 작업 중인 다른 에이전트가 구현 예정이므로
    /// Singleton 이 null 이면 내구도 바를 그대로 유지합니다.
    ///
    /// UIList: Panel_FishingHUD (Panel 범위)
    ///
    /// Inspector 와이어링:
    ///   helmImage      — 조타륜 Image (단순 회전)
    ///   durabilityBar  — Image, Fill Method = Horizontal, Fill Origin = Left
    ///   speedBar       — Image, Fill Method = Horizontal, Fill Origin = Left
    /// </summary>
    public class FishingHudUI : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Helm (Compass)")]
        [Tooltip("조타륜 이미지. FacingAngle 에 따라 Z 축으로 회전합니다.")]
        [SerializeField] private Image helmImage;

        [Header("Bars")]
        [Tooltip("내구도 바 (Image, Filled Horizontal). VesselHull 이 없으면 갱신 생략.")]
        [SerializeField] private Image durabilityBar;

        [Tooltip("속도 원형 (Image, Filled Horizontal). VesselController.SpeedRatio.")]
        [SerializeField] private RectTransform circle;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float minY = -55;
        [SerializeField] float maxY = 105;
        [SerializeField] float smoothTime = 0.08f;
        float currentYVelocity;
        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            base.Show();
            BindDurabilityEvent();
            // 첫 프레임 즉시 동기화
            SyncDurability();
        }

        public override void Hide()
        {
            UnbindDurabilityEvent();
            base.Hide();
        }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Update()
        {
            UpdateHelm();
            UpdateSpeedBar();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void UpdateHelm()
        {
            if (helmImage == null) return;
            if (VesselController.Singleton == null) return;

            // FacingAngle(World Y) → UI Z 회전 (반전: 선수가 위를 향할 때 0°)
            float angle = -VesselController.Singleton.FacingAngle;
            helmImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void UpdateSpeedBar()
        {
            if (circle == null) return;
            if (VesselController.Singleton == null) return;

            float clampedRatio = Mathf.Clamp01(VesselController.Singleton.SpeedRatio);
            float targetY = Mathf.Lerp(minY, maxY, clampedRatio);

            Vector2 pos = circle.anchoredPosition;
            pos.y = Mathf.SmoothDamp(pos.y, targetY, ref currentYVelocity, smoothTime);
            circle.anchoredPosition = pos;
        }

        // ── 내구도 이벤트 바인딩 ──────────────────────────────────────

        private void BindDurabilityEvent()
        {
#if !UNITY_EDITOR
            // VesselHull 은 아직 미구현일 수 있으므로 조건부 바인딩
#endif
            if (VesselHull.Singleton != null)
                VesselHull.Singleton.OnDurabilityChanged += HandleDurabilityChanged;
        }

        private void UnbindDurabilityEvent()
        {
            if (VesselHull.Singleton != null)
                VesselHull.Singleton.OnDurabilityChanged -= HandleDurabilityChanged;
        }

        private void HandleDurabilityChanged(float newDurability)
        {
            ApplyDurability(newDurability);
        }

        /// <summary>Show() 시점에 현재 내구도 값으로 즉시 동기화.</summary>
        private void SyncDurability()
        {
            if (VesselHull.Singleton == null) return;
            ApplyDurability(VesselHull.Singleton.CurrentDurability);
        }

        private void ApplyDurability(float durability)
        {
            if (durabilityBar == null) return;
            if (VesselHull.Singleton == null) return;

            float max = VesselHull.Singleton.MaxDurability;
            durabilityBar.fillAmount = (max > 0f) ? Mathf.Clamp01(durability / max) : 0f;
        }
    }
}
