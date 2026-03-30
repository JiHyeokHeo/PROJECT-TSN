using UnityEngine;

namespace TST
{
    /// <summary>
    /// 우주 공간에 떠다니는 이상 신호 포인트.
    /// 클릭하면 FocusMiniGameController 미니게임을 시작하고, 상호작용 후 비활성화됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SignalPoint : MonoBehaviour
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private ObservationZone zone;

        [Tooltip("기본 발견 보너스 배율 (1.0 = 보너스 없음)")]
        [SerializeField] private float baseDiscoveryBonus = 1.0f;

        // ── 런타임 상태 ──────────────────────────────────────────────
        public bool IsInteracted { get; private set; } = false;

        /// <summary>이 신호가 속한 관측 구역입니다.</summary>
        public ObservationZone Zone => zone;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>존재 구역과 소속 구역을 런타임에 설정합니다 (풀/스폰 패턴용).</summary>
        public void Initialize(ObservationZone targetZone)
        {
            zone         = targetZone;
            IsInteracted = false;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 망원경 렌즈 레벨을 반영한 발견 보너스를 반환합니다.
        /// </summary>
        public float GetDiscoveryBonus()
        {
            int lensLevel = TelescopeData.Singleton.GetLevel(TelescopePartType.Lens);
            // 렌즈 레벨당 5% 보너스
            return baseDiscoveryBonus * (1f + lensLevel * 0.05f);
        }

        // ── Unity 이벤트 ─────────────────────────────────────────────

        private void OnMouseDown()
        {
            if (IsInteracted) return;

            // 낚시 세션이 활성 상태일 때만 반응
            if (!FishingPhaseController.Singleton.IsActive) return;

            // 정선 상태일 때만 상호작용 가능
            if (VesselController.Singleton == null || !VesselController.Singleton.IsStationary) return;

            IsInteracted = true;
            FocusMiniGameController.Singleton.StartMinigame(this);

            // 스프라이트 페이드 등 시각 처리는 필요 시 여기서 추가
            gameObject.SetActive(false);
        }
    }
}
