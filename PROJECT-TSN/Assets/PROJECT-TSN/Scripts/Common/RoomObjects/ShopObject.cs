using UnityEngine;

namespace TST
{
    /// <summary>
    /// 도시 상점 오브젝트.
    /// DayCity 페이즈에서만 상호작용 가능. 클릭 시 상점 서브 로케이션을 엽니다.
    /// </summary>
    public class ShopObject : InteractableObject
    {
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
            IsInteractable = newPhase == GamePhase.DayCity;
        }

        protected override void OnInteract()
        {
            DayCityController.Singleton.ShowSubLocation(SubLocation.Shop);
        }
    }
}
