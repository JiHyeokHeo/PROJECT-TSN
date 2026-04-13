using UnityEngine;

namespace TST
{
    /// <summary>
    /// DayCity 페이즈 진입 시 활성화됩니다.
    /// 서브 로케이션 UI 패널 전환을 조율합니다.
    ///
    /// Inspector wiring:
    ///   - cityRoot : 도시 장면 루트 GameObject (활성/비활성 토글 대상)
    /// </summary>
    public class DayCityController : SingletonBase<DayCityController>
    {
        // ----------------------------------------------------------------
        //  Inspector
        // ----------------------------------------------------------------
        [Header("Scene Root (toggled by phase)")]
        [SerializeField] private GameObject cityRoot;

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private SubLocation _activeSubLocation = SubLocation.None;

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
            bool isDayCity = newPhase == GamePhase.DayCity;

            if (cityRoot != null)
                cityRoot.SetActive(isDayCity);

            if (!isDayCity)
                _activeSubLocation = SubLocation.None;
        }

        // ----------------------------------------------------------------
        //  Public API
        // ----------------------------------------------------------------
        /// <summary>
        /// 서브 로케이션 패널을 열고 RightFrame 배경을 갱신합니다.
        /// </summary>
        public void ShowSubLocation(SubLocation loc)
        {
            // 이전 서브 로케이션 닫기
            HideCurrentSubLocationPanel();

            _activeSubLocation = loc;

            switch (loc)
            {
                case SubLocation.Academy:
                    UIManager.Show<AcademyController>(UIList.Popup_RecordDisposal);
                    break;
                case SubLocation.University:
                    UIManager.Show<UniversityController>(UIList.Popup_TelescopeUpgrade);
                    break;
                case SubLocation.Shop:
                    UIManager.Show<DecorationShopController>(UIList.Popup_DecorationShop);
                    break;
            }

            RightFrameContentController.Singleton.SetSubLocation(loc);
        }

        /// <summary>
        /// 현재 열린 서브 로케이션을 닫고 DayCity 배경으로 복귀합니다.
        /// </summary>
        public void HideSubLocation()
        {
            HideCurrentSubLocationPanel();
            _activeSubLocation = SubLocation.None;
            RightFrameContentController.Singleton.SetSubLocation(SubLocation.None);
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        private void HideCurrentSubLocationPanel()
        {
            switch (_activeSubLocation)
            {
                case SubLocation.Academy:
                    UIManager.Hide<AcademyController>(UIList.Popup_RecordDisposal);
                    break;
                case SubLocation.University:
                    UIManager.Hide<UniversityController>(UIList.Popup_TelescopeUpgrade);
                    break;
                case SubLocation.Shop:
                    UIManager.Hide<DecorationShopController>(UIList.Popup_DecorationShop);
                    break;
            }
        }
    }
}
