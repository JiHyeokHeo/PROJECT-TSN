using UnityEngine;

namespace TST
{
    /// <summary>
    /// 다락방 문 오브젝트.
    /// DayAttic → DayCity 전환. 다른 페이즈에서는 비활성화됩니다.
    /// </summary>
    public class DoorObject : InteractableObject
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

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            IsInteractable = newPhase == GamePhase.DayAttic;
        }

        protected override void OnInteract()
        {
            PhaseManager.Singleton.TransitionTo(GamePhase.DayCity);
        }
    }
}
