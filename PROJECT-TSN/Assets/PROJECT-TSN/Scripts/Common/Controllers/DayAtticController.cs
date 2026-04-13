using UnityEngine;

namespace TST
{
    /// <summary>
    /// DayAttic 페이즈 진입 시 활성화되고, 다른 페이즈에서는 비활성화됩니다.
    /// 방 안 인터랙션 오브젝트(망원경/책장/침대/문)에 대한 중앙 조율 역할을 합니다.
    /// 각 RoomObject 는 내부적으로 PhaseManager 를 구독하여 자체 동작을 처리하므로,
    /// DayAtticController 는 오브젝트 참조를 보유하고 시각적 활성/비활성만 관리합니다.
    ///
    /// Inspector wiring:
    ///   - telescopeObj  : TelescopeObject  (다락방 망원경)
    ///   - bookshelfObj  : BookshelfObject  (책장)
    ///   - bedObj        : BedObject        (침대)
    ///   - doorObj       : InteractableObject (문 — DayCity 전환용)
    ///   - atticRoot     : 다락방 장면 루트 GameObject (활성/비활성 토글 대상)
    /// </summary>
    public class DayAtticController : SingletonBase<DayAtticController>
    {
        // ----------------------------------------------------------------
        //  Inspector
        // ----------------------------------------------------------------
        [Header("Interactable Objects")]
        [SerializeField] private TelescopeObject  telescopeObj;
        [SerializeField] private BookshelfObject  bookshelfObj;
        [SerializeField] private BedObject        bedObj;
        [SerializeField] private InteractableObject doorObj;

        [Header("Scene Root (toggled by phase)")]
        [SerializeField] private GameObject atticRoot;

        // ----------------------------------------------------------------
        //  Unity lifecycle
        // ----------------------------------------------------------------
        private void OnEnable()
        {
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        // ----------------------------------------------------------------
        //  Phase handler
        // ----------------------------------------------------------------
        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            bool isDayAttic = newPhase == GamePhase.DayAttic;

            if (atticRoot != null)
                atticRoot.SetActive(isDayAttic);

            // 문 오브젝트: DayAttic 에서만 인터랙션 가능
            if (doorObj != null)
                doorObj.IsInteractable = isDayAttic;
        }

        // ----------------------------------------------------------------
        //  Door interaction — 도시로 이동
        //  doorObj 의 OnInteract 를 외부에서 트리거하거나,
        //  문 오브젝트를 DoorObject : InteractableObject 로 만들면 자동 처리됩니다.
        //  여기서는 doorObj 에 DoorObject 스크립트가 없을 경우를 위한 fallback 을 제공합니다.
        // ----------------------------------------------------------------
        public void OnDoorClicked()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.DayAttic) return;
            PhaseManager.Singleton.TransitionTo(GamePhase.DayCity);
        }
    }
}
