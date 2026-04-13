using UnityEngine;

namespace TST
{
    /// <summary>
    /// 화면 9 (Space 페이즈) 진입/이탈 시 Space 전용 UI 요소를 켜고 끕니다.
    /// 항상 활성 상태인 GO(SpaceMapController 등)에 부착해야 합니다.
    /// </summary>
    public class SpaceSelectHUD : MonoBehaviour
    {
        [Header("Space UI Elements")]
        [Tooltip("Space 페이즈에서만 표시할 UI GameObject 목록 (UI_SpaceRadar, UI_FieldSelectHighlight 등)")]
        [SerializeField] private GameObject[] spaceUIElements;

        private void Awake()
        {
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
            SetSpaceUIActive(PhaseManager.Singleton.CurrentPhase == GamePhase.Space);
        }

        private void OnDestroy()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            SetSpaceUIActive(newPhase == GamePhase.Space);
        }

        private void SetSpaceUIActive(bool active)
        {
            foreach (var element in spaceUIElements)
            {
                if (element != null) element.SetActive(active);
            }
        }
    }
}
