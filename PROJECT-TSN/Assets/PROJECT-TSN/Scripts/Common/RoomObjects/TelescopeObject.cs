using UnityEngine;

namespace TST
{
    /// <summary>
    /// 다락방 망원경 오브젝트.
    /// 페이즈에 따라 업그레이드 팝업 열기 / 낚시(관측) 페이즈 진입 / 비활성화로 동작합니다.
    /// </summary>
    public class TelescopeObject : InteractableObject
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
            GamePhase current = PhaseManager.Singleton.CurrentPhase;

            switch (current)
            {
                case GamePhase.DayAttic:
                    UIManager.Show<UIBase>(UIList.Popup_TelescopeUpgrade);
                    break;
                case GamePhase.NightA:
                case GamePhase.NightB:
                    // 밤 망원경 팝업: 낚시 진입(NightA) / 엔딩 선택지 제공
                    UIManager.Show<NightTelescopePopupUI>(UIList.Popup_NightTelescope);
                    break;
            }
        }
    }
}
