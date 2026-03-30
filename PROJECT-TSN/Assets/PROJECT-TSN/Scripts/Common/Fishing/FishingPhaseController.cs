using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 메인 컨트롤러.
    /// 300초 타이머를 관리하고 페이즈 전환을 주도합니다.
    /// </summary>
    public class FishingPhaseController : SingletonBase<FishingPhaseController>
    {
        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>매 프레임 잔여 시간(초)을 전달합니다.</summary>
        public event Action<float> OnTimerUpdated;

        /// <summary>낚시 세션이 종료될 때 발행됩니다.</summary>
        public event Action OnFishingEnded;

        // ── 설정 필드 ────────────────────────────────────────────────
        [SerializeField] private float totalTime = 300f;

        // ── 런타임 상태 ──────────────────────────────────────────────
        public float            TotalTime      => totalTime;
        public float            RemainingTime  { get; private set; }
        public bool             IsActive       { get; private set; }
        public ObservationZone  CurrentZone    { get; private set; }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>구역을 지정해 낚시 세션을 시작합니다.</summary>
        public void StartFishing(ObservationZone zone)
        {
            if (zone == null)
            {
                Debug.LogError("[FishingPhaseController] StartFishing: zone이 null입니다.");
                return;
            }

            CurrentZone   = zone;
            RemainingTime = totalTime;
            IsActive      = true;

            Debug.Log($"[FishingPhaseController] 낚시 시작 — 구역: {zone.zoneName}");
        }

        /// <summary>타이머 만료 시 자동 호출되는 정상 종료 경로입니다.</summary>
        public void EndFishing()
        {
            if (!IsActive) return;

            IsActive = false;

            Debug.Log("[FishingPhaseController] 낚시 종료 — NightB 페이즈로 전환.");

            OnFishingEnded?.Invoke();
            PhaseManager.Singleton.TransitionTo(GamePhase.NightB);
        }

        /// <summary>일루산 타이머 클릭 등 즉시 종료 경로입니다.</summary>
        public void SkipFishing()
        {
            RemainingTime = 0f;
            EndFishing();
        }

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Update()
        {
            if (!IsActive) return;

            RemainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                EndFishing();
            }
        }
    }
}
