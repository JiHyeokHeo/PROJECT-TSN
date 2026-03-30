using UnityEngine;

namespace TST
{
    /// <summary>
    /// 다락방 책장 오브젝트.
    /// 낮 → 천체 도감 팝업, 밤 → 인벤토리 팝업.
    /// </summary>
    public class BookshelfObject : InteractableObject
    {
        private bool _isDay = true;

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
                case GamePhase.DayCity:
                    _isDay = true;
                    IsInteractable = true;
                    break;
                case GamePhase.NightA:
                case GamePhase.NightB:
                    _isDay = false;
                    IsInteractable = true;
                    break;
                default:
                    IsInteractable = false;
                    break;
            }
        }

        protected override void OnInteract()
        {
            if (_isDay)
                UIManager.Show<UIBase>(UIList.Popup_ObservationJournal);
            else
                UIManager.Show<UIBase>(UIList.Popup_Inventory);
        }
    }
}
