using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈의 존재 일루산 오브젝트.
    /// NightA/NightB에서 코즈믹 트레이스 기록이 있을 때만 꿈 페이즈로 전환합니다.
    /// </summary>
    public class IlusanObject : InteractableObject
    {
        private GamePhase _currentPhase = GamePhase.DayAttic;

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
            _currentPhase = newPhase;

            switch (newPhase)
            {
                case GamePhase.NightA:
                case GamePhase.NightB:
                    // 코즈믹 트레이스 보유 여부로 활성 여부 결정
                    IsInteractable = ObservationJournal.Singleton.HasCosmicTrace;
                    break;
                default:
                    // 낮 또는 기타 페이즈에서는 비활성
                    IsInteractable = false;
                    break;
            }
        }

        protected override void OnInteract()
        {
            // 이중 검증: 페이즈 진입 시점에 변경될 수 있으므로 재확인
            if (_currentPhase != GamePhase.NightA && _currentPhase != GamePhase.NightB)
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
