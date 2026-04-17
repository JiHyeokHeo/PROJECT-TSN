using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 메인 컨트롤러.
    /// 300초 타이머를 관리하고 페이즈 전환을 주도합니다.
    ///
    /// 타이머 감소 조건 (둘 중 하나 이상 해당 시 감소):
    ///   - 관측선이 이동 중 (VesselController.SpeedRatio > 0)
    ///   - 낚시 미니게임 수행 중 (FocusMiniGameController.State == Active)
    ///
    /// 타이머 정지 조건 (위 조건 충족 중이어도 우선 정지):
    ///   - 메뉴 팝업 열림 (Popup_Menu)
    ///   - SkipFishing 경고 팝업 표시 중 (IsSkipConfirmShowing)
    ///   - 인벤토리 / 관측기록 팝업 열림
    /// </summary>
    public class FishingPhaseController : MonoBehaviour
    {
        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>매 프레임 잔여 시간(초)을 전달합니다.</summary>
        public event Action<float> OnTimerUpdated;

        /// <summary>낚시 세션이 종료될 때 발행됩니다.</summary>
        public event Action OnFishingEnded;

        // ── 설정 필드 ────────────────────────────────────────────────
        [SerializeField] private float          totalTime      = 300f;

        [Header("Dependencies")]
        [SerializeField] private VesselController         vesselController;
        [SerializeField] private FocusMiniGameController  focusMiniGameController;

        // ── 런타임 상태 ──────────────────────────────────────────────
        public float            TotalTime             => totalTime;
        public float            RemainingTime         { get; private set; }
        public bool             IsActive              { get; private set; }
        public bool             IsSkipConfirmShowing  { get; set; }
        public ObservationZone  CurrentZone           { get; private set; }

        // ── 캐시된 UI 참조 (Awake에서 한 번만 조회) ───────────────────
        private UIBase _menuPopup;
        private UIBase _inventoryPopup;
        private UIBase _journalPopup;

        private void Awake()
        {
            _menuPopup      = UIManager.Singleton?.GetUI<UIBase>(UIList.Popup_Menu);
            _inventoryPopup = UIManager.Singleton?.GetUI<UIBase>(UIList.Popup_Inventory);
            _journalPopup   = UIManager.Singleton?.GetUI<UIBase>(UIList.Popup_ObservationJournal);
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>구역을 지정해 낚시 세션을 시작합니다.</summary>
        public void StartFishing(ObservationZone zone)
        {
            if (zone == null)
            {
                Debug.LogError("[FishingPhaseController] StartFishing: zone이 null입니다.");
                return;
            }

            CurrentZone          = zone;
            RemainingTime        = totalTime;
            IsActive             = true;
            IsSkipConfirmShowing = false;

            UIManager.Show<FishingHudUI>(UIList.Panel_FishingHUD);
            UIManager.Show<FishingTimerUI>(UIList.Panel_FishingTimer);

            Debug.Log($"[FishingPhaseController] 낚시 시작 — 구역: {zone.zoneName}");
        }

        /// <summary>타이머 만료 시 자동 호출되는 정상 종료 경로입니다.</summary>
        public void EndFishing()
        {
            if (!IsActive) return;

            IsActive = false;

            UIManager.Hide<FishingHudUI>(UIList.Panel_FishingHUD);
            UIManager.Hide<FishingTimerUI>(UIList.Panel_FishingTimer);
            Debug.Log("[FishingPhaseController] 낚시 종료 — NightB 페이즈로 전환.");

            // PhaseManager.TransitionTo(NightB)는 FishingTransitionController.FishingExitRoutine에서
            // 암전 + 씬 오브젝트 정리 후 호출됩니다.
            OnFishingEnded?.Invoke();
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

            if (ShouldTickTimer())
            {
                RemainingTime -= Time.deltaTime;
                OnTimerUpdated?.Invoke(RemainingTime);

                if (RemainingTime <= 0f)
                {
                    RemainingTime = 0f;
                    EndFishing();
                }
            }
        }

        // ── 내부 ─────────────────────────────────────────────────────

        /// <summary>
        /// 타이머를 소모할지 여부를 판단합니다.
        /// 정지 조건이 우선하며, 이동 또는 미니게임 중일 때만 감소합니다.
        /// </summary>
        private bool ShouldTickTimer()
        {
            // ── 정지 조건 확인 (우선순위 높음) ────────────────────────
            // 메뉴 팝업 열림
            if (_menuPopup != null && _menuPopup.gameObject.activeSelf) return false;

            // SkipFishing 경고 팝업 표시 중
            if (IsSkipConfirmShowing) return false;

            // 인벤토리 팝업 열림
            if (_inventoryPopup != null && _inventoryPopup.gameObject.activeSelf) return false;

            // 관측기록 팝업 열림
            if (_journalPopup != null && _journalPopup.gameObject.activeSelf) return false;

            // ── 감소 조건 확인 (둘 중 하나 이상 해당) ─────────────────
            bool isMoving   = vesselController != null && vesselController.SpeedRatio > 0f;
            bool isMiniGame = focusMiniGameController != null
                              && focusMiniGameController.State == FocusMiniGameController.MiniGameState.Active;

            return isMoving || isMiniGame;
        }
    }
}
