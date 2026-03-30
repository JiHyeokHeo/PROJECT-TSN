using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 관측선 내구도 시스템.
    /// 위험 요소 충돌로 감소, 기록 보관소 상호작용으로 회복됩니다.
    /// 내구도 0 도달 시 FishingPhaseController.EndFishing()을 통해 침몰 처리합니다.
    /// </summary>
    public class VesselHull : SingletonBase<VesselHull>
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("Durability")]
        [Tooltip("최대 내구도")]
        [SerializeField] private float maxDurability = 100f;

        [Tooltip("낚시 세션 시작 시 내구도를 최대치로 초기화할지 여부")]
        [SerializeField] private bool resetOnSessionStart = true;

        // ── 이벤트 ───────────────────────────────────────────────────
        /// <summary>내구도가 변경될 때마다 현재 값을 전달합니다 (0~maxDurability).</summary>
        public event Action<float> OnDurabilityChanged;

        /// <summary>내구도가 0 이하로 떨어져 침몰이 발생할 때 발행됩니다.</summary>
        public event Action OnSunk;

        // ── 프로퍼티 ─────────────────────────────────────────────────
        public float MaxDurability    => maxDurability;
        public float CurrentDurability { get; private set; }

        /// <summary>0~1 정규화 비율 (UI 게이지용).</summary>
        public float DurabilityRatio  => CurrentDurability / maxDurability;

        // ── 침몰 중복 방지 플래그 ────────────────────────────────────
        private bool _isSunk;

        // ─────────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            CurrentDurability = maxDurability;
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>세션 시작 시 내구도를 최대치로 초기화합니다.</summary>
        public void InitializeForSession()
        {
            if (!resetOnSessionStart) return;

            _isSunk           = false;
            CurrentDurability = maxDurability;
            OnDurabilityChanged?.Invoke(CurrentDurability);
        }

        /// <summary>피해를 입혀 내구도를 감소시킵니다. 0 이하면 침몰 처리합니다.</summary>
        public void TakeDamage(float amount)
        {
            if (_isSunk) return;
            if (amount <= 0f) return;

            CurrentDurability = Mathf.Max(0f, CurrentDurability - amount);
            OnDurabilityChanged?.Invoke(CurrentDurability);

            Debug.Log($"[VesselHull] 피해 -{amount:F1} → 현재 내구도: {CurrentDurability:F1}/{maxDurability:F1}");

            if (CurrentDurability <= 0f)
                HandleSunk();
        }

        /// <summary>내구도를 회복합니다. 최대치를 초과하지 않습니다.</summary>
        public void Recover(float amount)
        {
            if (_isSunk) return;
            if (amount <= 0f) return;

            CurrentDurability = Mathf.Min(maxDurability, CurrentDurability + amount);
            OnDurabilityChanged?.Invoke(CurrentDurability);

            Debug.Log($"[VesselHull] 회복 +{amount:F1} → 현재 내구도: {CurrentDurability:F1}/{maxDurability:F1}");
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void HandleSunk()
        {
            _isSunk = true;
            Debug.Log("[VesselHull] 내구도 0 — 침몰 처리.");

            OnSunk?.Invoke();

            // 관측선을 즉시 정지
            VesselController.Singleton?.ForceStop();

            // 낚시 페이즈 종료 (침몰 = 강제 종료와 동일 경로)
            FishingPhaseController fc = FishingPhaseController.Singleton;
            if (fc != null && fc.IsActive)
                fc.EndFishing();
        }
    }
}
