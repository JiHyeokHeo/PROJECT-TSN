using UnityEngine;

namespace TST
{
    /// <summary>
    /// 다락방 침대 오브젝트.
    /// DayAttic → NightA 전환 / NightA → DayAttic / NightB → Dream 전환.
    /// </summary>
    public class BedObject : InteractableObject
    {
        protected override void Awake()
        {
            base.Awake();
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            switch (newPhase)
            {
                case GamePhase.DayAttic:
                case GamePhase.NightA:
                case GamePhase.NightB:
                    IsInteractable = true;
                    break;
                default:
                    IsInteractable = false;
                    break;
            }
        }

        protected override void OnInteract()
        {
            switch (PhaseManager.Singleton.CurrentPhase)
            {
                case GamePhase.DayAttic:
                    PhaseManager.Singleton.TransitionTo(GamePhase.NightA);
                    break;
                case GamePhase.NightA:
                    PhaseManager.Singleton.TransitionTo(GamePhase.DayAttic);
                    break;
                case GamePhase.NightB:
                    // Panel_Dream 을 먼저 Show 해 OnEnable 구독을 보장한 뒤 페이즈를 전환합니다.
                    //UIManager.Show<DreamBaseController>(UIList.Panel_Dream);
                    PhaseManager.Singleton.TransitionTo(GamePhase.Dream);
                    break;
            }
        }
    }
}
