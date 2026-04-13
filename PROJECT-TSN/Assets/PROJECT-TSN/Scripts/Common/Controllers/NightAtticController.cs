using UnityEngine;

namespace TST
{
    /// <summary>
    /// 밤 다락방 씬 컨트롤러.
    /// PhaseManager.OnPhaseChanged를 구독하여 NightA / NightB 진입 시 활성화합니다.
    ///
    /// 오브젝트별 상호작용 분담:
    ///   망원경 → TelescopeObject.OnInteract (Popup_NightTelescope)
    ///   침대   → BedObject.OnInteract (GamePhase.Dream)
    ///   책장   → NightAtticController가 직접 담당 (Popup_ObservationJournal)
    ///
    /// Inspector 와이어링:
    ///   bookshelfObject — 책장 InteractableObject
    /// </summary>
    public class NightAtticController : SingletonBase<NightAtticController>
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Room Object References")]
        [SerializeField] private InteractableObject bookshelfObject;

        [Header("Scene Root (toggled by phase)")]
        [SerializeField] private GameObject atticRoot;

        // ── Unity 생명주기 ───────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;

            UnbindBookshelf();
        }

        // ── 페이즈 처리 ──────────────────────────────────────────────

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            bool isNight = newPhase == GamePhase.NightA || newPhase == GamePhase.NightB;

            if (atticRoot != null)
                atticRoot.SetActive(isNight);

            if (isNight)
                BindBookshelf();
            else
                UnbindBookshelf();
        }

        // ── 책장 바인딩 ──────────────────────────────────────────────

        private void BindBookshelf()
        {
            if (bookshelfObject == null) return;
            bookshelfObject.OnInteracted -= OnBookshelfInteracted;
            bookshelfObject.OnInteracted += OnBookshelfInteracted;
            bookshelfObject.IsInteractable = true;
        }

        private void UnbindBookshelf()
        {
            if (bookshelfObject == null) return;
            bookshelfObject.OnInteracted -= OnBookshelfInteracted;
        }

        // ── 상호작용 핸들러 ──────────────────────────────────────────

        private void OnBookshelfInteracted()
        {
            UIManager.Show<UIBase>(UIList.Popup_ObservationJournal);
        }
    }
}
