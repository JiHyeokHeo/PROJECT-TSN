using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈의 존재 일루산 오브젝트.
    /// NightA/NightB에서 코즈믹 트레이스 기록이 있을 때만 꿈 페이즈로 전환합니다.
    /// </summary>
    public class IlusanObject : InteractableObject
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
                case GamePhase.NightA:
                case GamePhase.NightB:
                    IsInteractable = ObservationJournal.Singleton.HasCosmicTrace;
                    break;
                default:
                    IsInteractable = false;
                    break;
            }
        }

        protected override void OnInteract()
        {
            var phase = PhaseManager.Singleton.CurrentPhase;
            if (phase != GamePhase.NightA && phase != GamePhase.NightB)
                return;

            if (!ObservationJournal.Singleton.HasCosmicTrace)
            {
                IsInteractable = false;
                return;
            }

            PhaseManager.Singleton.TransitionTo(GamePhase.Dream);
        }
    }
}
