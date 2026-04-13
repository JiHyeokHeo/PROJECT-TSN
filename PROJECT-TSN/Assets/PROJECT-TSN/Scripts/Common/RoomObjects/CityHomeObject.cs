using UnityEngine;

namespace TST
{
    /// <summary>
    /// 도시 집(귀가) 오브젝트.
    /// DayCity 페이즈에서만 상호작용 가능. 클릭 시 서브 로케이션을 닫고 DayAttic으로 전환합니다.
    /// </summary>
    /// 
    // 구독 시스템을 몇개까지 홀딩할지 고려해보자.
    public class CityHomeObject : InteractableObject
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
            IsInteractable = newPhase == GamePhase.DayCity;
        }

        protected override void OnInteract()
        {
            DayCityController.Singleton.HideSubLocation();
            PhaseManager.Singleton.TransitionTo(GamePhase.DayAttic);
        }
    }
}
